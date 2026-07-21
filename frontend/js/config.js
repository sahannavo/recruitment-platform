// ============================================
// js/config.js - Environment Configuration
// ============================================

const CONFIG = {
  // API Configuration
  API: {
    BASE_URL:
      window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1" || window.location.hostname === ""
        ? "http://localhost:5000" // Development
        : "https://api.yourdomain.com", // Production
    TIMEOUT: 30000,
    MAX_RETRIES: 2,
  },

  // Auth Configuration
  AUTH: {
    TOKEN_KEY: "accessToken",
    USER_KEY: "user",
  },

  // App Configuration
  APP: {
    NAME: "Recruitment Platform",
    VERSION: "1.0.0",
  },
};

// Freeze to prevent modifications
Object.freeze(CONFIG);

// Export for use
if (typeof module !== "undefined" && module.exports) {
  module.exports = CONFIG;
}
