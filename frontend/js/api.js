// ============================================
// js/api.js - API Client for Backend Communication
// ============================================

const API_BASE_URL = "https://localhost:5001";
const REQUEST_TIMEOUT = 30000; // 30 seconds
const MAX_RETRIES = 2;

// Get JWT token from localStorage
function getToken() {
  return localStorage.getItem("accessToken");
}

// Save JWT token
function setToken(token) {
  localStorage.setItem("accessToken", token);
}

// Remove JWT token (logout)
function removeToken() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("user");
}

// Get current user from localStorage
function getCurrentUser() {
  const user = localStorage.getItem("user");
  return user ? JSON.parse(user) : null;
}

// Save current user
function setCurrentUser(user) {
  localStorage.setItem("user", JSON.stringify(user));
}

// ============================================
// API Request Helper (FULLY FIXED)
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

  // Add Authorization header if token exists and requiresAuth is true
  if (requiresAuth) {
    const token = getToken();
    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    } else {
      // Redirect to login if no token
      if (
        !window.location.pathname.includes("login.html") &&
        !window.location.pathname.includes("register.html")
      ) {
        window.location.href = "/auth/login.html";
        return;
      }
    }
  }

  // Setup AbortController for timeout
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

  // Retry logic
  for (let attempt = 0; attempt < retries; attempt++) {
    try {
      const response = await fetch(url, options);
      clearTimeout(timeoutId);

      // Handle 401 Unauthorized
      if (response.status === 401) {
        removeToken();
        if (!window.location.pathname.includes("login.html")) {
          window.location.href = "/auth/login.html";
        }
        return null;
      }

      // Handle 204 No Content
      if (response.status === 204) {
        return { success: true };
      }

      const responseData = await response.json();

      if (!response.ok) {
        // ✅ FIXED: Check for 'detail' first (from backend ProblemDetails)
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

      // Retry on network errors only (not on AbortError or 4xx/5xx responses)
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

      // Throw error if it's AbortError (timeout)
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

  // ✅ FIX: Expose auth check functions
  isAuthenticated: isAuthenticated,
  getUserRole: getUserRole,
  getUserInitials: getUserInitials,
  getUserFullName: getUserFullName,
};

// ============================================
// UPDATE EXPOSED API
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
};

// ============================================
// FEEDBACK API
// ============================================
const FeedbackAPI = {
  submit: async (feedbackData) => {
    return await apiRequest("/api/feedbacks", "POST", feedbackData, true);
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

  getAnalytics: async () => {
    return await apiRequest("/api/analytics/recruitment", "GET", null, true);
  },

  getHealth: async () => {
    return await apiRequest("/api/system/health", "GET", null, true);
  },
};

// ============================================
// EXPOSE TO GLOBAL SCOPE
// ============================================


console.log("✅ API Client Loaded Successfully!");
console.log("📍 API Base URL:", API_BASE_URL);
console.log(`⏱️  Timeout: ${REQUEST_TIMEOUT / 1000}s, Retries: ${MAX_RETRIES}`);
