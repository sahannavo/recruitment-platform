// ============================================
// js/api.js - API Client for Backend Communication
// ============================================

const API_BASE_URL = "https://localhost:5001";
const REQUEST_TIMEOUT = 30000; // 30 seconds
const MAX_RETRIES = 2;

// ============================================
// TOKEN MANAGEMENT
// ============================================
function getToken() {
  return localStorage.getItem("accessToken");
}

function setToken(token) {
  localStorage.setItem("accessToken", token);
}

function removeToken() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("user");
}

function getCurrentUser() {
  const user = localStorage.getItem("user");
  return user ? JSON.parse(user) : null;
}

function setCurrentUser(user) {
  localStorage.setItem("user", JSON.stringify(user));
}

// ============================================
// ✅ RESTORED: AUTH HELPER FUNCTIONS
// ============================================
function isAuthenticated() {
  const token = getToken();
  if (!token) return false;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    return Date.now() < payload.exp * 1000;
  } catch {
    return false;
  }
}

function getUserRole() {
  const user = getCurrentUser();
  return user?.role || user?.Role || "Candidate";
}

function getUserInitials() {
  const user = getCurrentUser();
  if (!user) return "U";
  const firstName = user.firstName || user.FirstName || "";
  const lastName = user.lastName || user.LastName || "";
  if (firstName && lastName) {
    return `${firstName[0]}${lastName[0]}`.toUpperCase();
  }
  return firstName[0]?.toUpperCase() || "U";
}

function getUserFullName() {
  const user = getCurrentUser();
  if (!user) return "User";
  const firstName = user.firstName || user.FirstName || "";
  const lastName = user.lastName || user.LastName || "";
  if (firstName && lastName) {
    return `${firstName} ${lastName}`;
  }
  return firstName || "User";
}

// ============================================
// API REQUEST HELPER
// ============================================
async function apiRequest(
  endpoint,
  method = "GET",
  data = null,
  requiresAuth = true,
  retries = MAX_RETRIES,
) {
  const url = `${API_BASE_URL}${endpoint}`;

  const headers = {
    "Content-Type": "application/json",
    Accept: "application/json",
  };

  if (requiresAuth) {
    const token = getToken();
    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    } else {
      if (
        !window.location.pathname.includes("login.html") &&
        !window.location.pathname.includes("register.html")
      ) {
        window.location.href = "/auth/login.html";
        return;
      }
    }
  }

  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT);

  const options = {
    method: method,
    headers: headers,
    signal: controller.signal,
  };

  if (data) {
    options.body = JSON.stringify(data);
  }

  for (let attempt = 0; attempt < retries; attempt++) {
    try {
      const response = await fetch(url, options);
      clearTimeout(timeoutId);

      if (response.status === 401) {
        removeToken();
        if (!window.location.pathname.includes("login.html")) {
          window.location.href = "/auth/login.html";
        }
        return null;
      }

      if (response.status === 204) {
        return { success: true };
      }

      const responseData = await response.json();

      if (!response.ok) {
        const errorMsg =
          responseData.detail ||
          responseData.message ||
          responseData.title ||
          "API request failed";
        throw new Error(errorMsg);
      }

      return responseData;
    } catch (error) {
      clearTimeout(timeoutId);

      const shouldRetry =
        attempt < retries - 1 &&
        (error.name === "TypeError" ||
          error.name === "NetworkError" ||
          error.message.includes("Failed to fetch"));

      if (shouldRetry) {
        console.warn(`Retrying request (${attempt + 1}/${retries})...`);
        await new Promise((resolve) =>
          setTimeout(resolve, 1000 * (attempt + 1)),
        );
        continue;
      }

      if (error.name === "AbortError") {
        throw new Error(
          `Request timed out after ${REQUEST_TIMEOUT / 1000} seconds`,
        );
      }

      console.error("API Error:", error);
      throw error;
    }
  }
}

// ============================================
// ✅ RESTORED: CANDIDATE API
// ============================================
const CandidateAPI = {
  getProfile: async () => {
    return await apiRequest("/api/candidates/profile", "GET", null, true);
  },

  updateProfile: async (profileData) => {
    return await apiRequest(
      "/api/candidates/profile",
      "PUT",
      profileData,
      true,
    );
  },

  uploadCV: async (file) => {
    const token = getToken();
    const formData = new FormData();
    formData.append("file", file);

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT);

    try {
      const response = await fetch(`${API_BASE_URL}/api/candidates/upload-cv`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        body: formData,
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.detail || error.message || "Upload failed");
      }

      return await response.json();
    } catch (error) {
      clearTimeout(timeoutId);
      if (error.name === "AbortError") {
        throw new Error(
          `Upload timed out after ${REQUEST_TIMEOUT / 1000} seconds`,
        );
      }
      console.error("Upload Error:", error);
      throw error;
    }
  },
};

// ============================================
// AUTH API (FIXED)
// ============================================
const AuthAPI = {
  login: async (email, password) => {
    const response = await apiRequest(
      "/api/auth/login",
      "POST",
      { email, password },
      false,
    );
    if (response && response.token) {
      setToken(response.token);

      // ✅ FIX: Transform backend flat response to frontend user object
      const user = {
        userId: response.userId || response.UserId,
        email: response.email || response.Email,
        firstName: response.firstName || response.FirstName || "",
        lastName: response.lastName || response.LastName || "",
        role: response.role || response.Role || "Candidate",
        expiresAt: response.expiresAt || response.ExpiresAt,
      };

      setCurrentUser(user);
      return response;
    }
    return null;
  },

  register: async (userData) => {
    return await apiRequest("/api/auth/register", "POST", userData, false);
  },

  logout: () => {
    removeToken();
    window.location.href = "/auth/login.html";
  },

  getMe: async () => {
    return await apiRequest("/api/auth/me", "GET", null, true);
  },

  // ✅ Expose auth check functions
  isAuthenticated: isAuthenticated,
  getUserRole: getUserRole,
  getUserInitials: getUserInitials,
  getUserFullName: getUserFullName,
};

// ============================================
// JOB API
// ============================================
const JobAPI = {
  getAll: async (filters = {}) => {
    const queryParams = new URLSearchParams(filters).toString();
    const endpoint = `/api/jobs${queryParams ? "?" + queryParams : ""}`;
    return await apiRequest(endpoint, "GET", null, true);
  },

  getRecommended: async () => {
    return await apiRequest("/api/jobs/recommended", "GET", null, true);
  },

  getById: async (jobId) => {
    return await apiRequest(`/api/jobs/${jobId}`, "GET", null, true);
  },

  create: async (jobData) => {
    return await apiRequest("/api/jobs", "POST", jobData, true);
  },

  update: async (jobId, jobData) => {
    return await apiRequest(`/api/jobs/${jobId}`, "PUT", jobData, true);
  },

  delete: async (jobId) => {
    return await apiRequest(`/api/jobs/${jobId}`, "DELETE", null, true);
  },

  getStatistics: async () => {
    return await apiRequest("/api/jobs/statistics", "GET", null, true);
  },
};

// ============================================
// APPLICATION API
// ============================================
const ApplicationAPI = {
  submit: async (applicationData) => {
    return await apiRequest("/api/applications", "POST", applicationData, true);
  },

  getMyApplications: async () => {
    return await apiRequest("/api/applications/candidate", "GET", null, true);
  },

  getByJob: async (jobId) => {
    return await apiRequest(
      `/api/applications/recruiter/${jobId}`,
      "GET",
      null,
      true,
    );
  },

  updateStatus: async (applicationId, statusData) => {
    return await apiRequest(
      `/api/applications/${applicationId}/status`,
      "PUT",
      statusData,
      true,
    );
  },

  getById: async (applicationId) => {
    return await apiRequest(`/api/applications/${applicationId}`, "GET", null, true);
  },

  getStatistics: async () => {
    return await apiRequest("/api/applications/statistics", "GET", null, true);
  },

  withdraw: async (applicationId) => {
    return await apiRequest(
      `/api/applications/${applicationId}/withdraw`,
      "PUT",
      null,
      true,
    );
  },
};

// ============================================
// INTERVIEW API
// ============================================
const InterviewAPI = {
  schedule: async (interviewData) => {
    return await apiRequest(
      "/api/interviews/schedule",
      "POST",
      interviewData,
      true,
    );
  },

  getMyInterviews: async () => {
    return await apiRequest("/api/interviews/my", "GET", null, true);
  },

  getAvailability: async (params = {}) => {
    const queryParams = new URLSearchParams(params).toString();
    const endpoint = `/api/interviews/availability${queryParams ? "?" + queryParams : ""}`;
    return await apiRequest(endpoint, "GET", null, true);
  },
};

// ============================================
// FEEDBACK API
// ============================================
const FeedbackAPI = {
  submit: async (feedbackData) => {
    return await apiRequest("/api/feedbacks", "POST", feedbackData, true);
  },

  getMyFeedback: async () => {
    return await apiRequest("/api/feedbacks/manager", "GET", null, true);
  },

  getById: async (feedbackId) => {
    return await apiRequest(`/api/feedbacks/${feedbackId}`, "GET", null, true);
  },

  update: async (feedbackId, feedbackData) => {
    return await apiRequest(`/api/feedbacks/${feedbackId}`, "PUT", feedbackData, true);
  },
};

// ============================================
// ADMIN API
// ============================================
const AdminAPI = {
  getUsers: async () => {
    return await apiRequest("/api/admin/users", "GET", null, true);
  },

  updateUserRole: async (userId, roleData) => {
    return await apiRequest(
      `/api/admin/users/${userId}/role`,
      "PUT",
      roleData,
      true,
    );
  },

  getAnalytics: async (params = "") => {
    return await apiRequest(`/api/analytics/recruitment?${params}`, "GET", null, true);
  },

  getDashboard: async () => {
    return await apiRequest("/api/admin/dashboard", "GET", null, true);
  },

  getAuditLogs: async () => {
    return await apiRequest("/api/admin/audit-logs", "GET", null, true);
  },

  disableUser: async (userId) => {
    return await apiRequest(`/api/admin/users/${userId}/disable`, "PUT", null, true);
  },

  enableUser: async (userId) => {
    return await apiRequest(`/api/admin/users/${userId}/enable`, "PUT", null, true);
  },

  deleteUser: async (userId) => {
    return await apiRequest(`/api/admin/users/${userId}`, "DELETE", null, true);
  },

  inviteUser: async (inviteData) => {
    return await apiRequest("/api/admin/users/invite", "POST", inviteData, true);
  },

  getHealth: async () => {
    return await apiRequest("/api/analytics/system-health", "GET", null, true);
  },
};

// ============================================
// EXPOSE TO GLOBAL SCOPE (SINGLE DEFINITION)
// ============================================
window.API = {
  Auth: AuthAPI,
  Candidate: CandidateAPI,
  Job: JobAPI,
  Application: ApplicationAPI,
  Interview: InterviewAPI,
  Feedback: FeedbackAPI,
  Admin: AdminAPI,
  getToken,
  setToken,
  removeToken,
  getCurrentUser,
  setCurrentUser,
  isAuthenticated,
  getUserRole,
  getUserInitials,
  getUserFullName,
};

console.log("✅ API Client Loaded Successfully!");
console.log("📍 API Base URL:", API_BASE_URL);
console.log(`⏱️  Timeout: ${REQUEST_TIMEOUT / 1000}s, Retries: ${MAX_RETRIES}`);