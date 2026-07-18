// ============================================
// AUTH API (UPDATED)
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

      // ✅ FIX: Transform backend response to frontend format
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

  // ✅ ADD: Check if authenticated
  isAuthenticated: isAuthenticated,

  // ✅ ADD: Get user role
  getUserRole: getUserRole,

  // ✅ ADD: Get user initials
  getUserInitials: getUserInitials,

  // ✅ ADD: Get user full name
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
  // ✅ ADD: Expose auth check functions
  isAuthenticated,
  getUserRole,
  getUserInitials,
  getUserFullName,
};
