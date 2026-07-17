// ============================================
// js/api.js - API Client for Backend Communication
// ============================================

const API_BASE_URL = 'https://localhost:7001'; // Your backend URL

// Get JWT token from localStorage
function getToken() {
    return localStorage.getItem('accessToken');
}

// Save JWT token
function setToken(token) {
    localStorage.setItem('accessToken', token);
}

// Remove JWT token (logout)
function removeToken() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('user');
}

// Get current user from localStorage
function getCurrentUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
}

// Save current user
function setCurrentUser(user) {
    localStorage.setItem('user', JSON.stringify(user));
}

// ============================================
// API Request Helper
// ============================================
async function apiRequest(endpoint, method = 'GET', data = null, requiresAuth = true) {
    const url = `${API_BASE_URL}${endpoint}`;
    
    const headers = {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    };
    
    // Add Authorization header if token exists and requiresAuth is true
    if (requiresAuth) {
        const token = getToken();
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        } else {
            // Redirect to login if no token
            if (!window.location.pathname.includes('login.html') && 
                !window.location.pathname.includes('register.html')) {
                window.location.href = '/auth/login.html';
                return;
            }
        }
    }
    
    const options = {
        method: method,
        headers: headers,
    };
    
    if (data) {
        options.body = JSON.stringify(data);
    }
    
    try {
        const response = await fetch(url, options);
        
        // Handle 401 Unauthorized
        if (response.status === 401) {
            removeToken();
            if (!window.location.pathname.includes('login.html')) {
                window.location.href = '/auth/login.html';
            }
            return null;
        }
        
        // Handle 204 No Content
        if (response.status === 204) {
            return { success: true };
        }
        
        const responseData = await response.json();
        
        if (!response.ok) {
            throw new Error(responseData.message || 'API request failed');
        }
        
        return responseData;
        
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// ============================================
// AUTH API
// ============================================
const AuthAPI = {
    login: async (email, password) => {
        const response = await apiRequest('/api/auth/login', 'POST', { email, password }, false);
        if (response && response.token) {
            setToken(response.token);
            setCurrentUser(response.user);
            return response;
        }
        return null;
    },
    
    register: async (userData) => {
        return await apiRequest('/api/auth/register', 'POST', userData, false);
    },
    
    logout: () => {
        removeToken();
        window.location.href = '/auth/login.html';
    },
    
    getMe: async () => {
        return await apiRequest('/api/auth/me', 'GET', null, true);
    }
};

// ============================================
// CANDIDATE API
// ============================================
const CandidateAPI = {
    getProfile: async () => {
        return await apiRequest('/api/candidates/profile', 'GET', null, true);
    },
    
    updateProfile: async (profileData) => {
        return await apiRequest('/api/candidates/profile', 'PUT', profileData, true);
    },
    
    uploadCV: async (file) => {
        const token = getToken();
        const formData = new FormData();
        formData.append('file', file);
        
        const response = await fetch(`${API_BASE_URL}/api/candidates/upload-cv`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            body: formData
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Upload failed');
        }
        
        return await response.json();
    }
};

// ============================================
// JOB API
// ============================================
const JobAPI = {
    getAll: async (filters = {}) => {
        const queryParams = new URLSearchParams(filters).toString();
        const endpoint = `/api/jobs${queryParams ? '?' + queryParams : ''}`;
        return await apiRequest(endpoint, 'GET', null, true);
    },
    
    getRecommended: async () => {
        return await apiRequest('/api/jobs/recommended', 'GET', null, true);
    },
    
    getById: async (jobId) => {
        return await apiRequest(`/api/jobs/${jobId}`, 'GET', null, true);
    },
    
    create: async (jobData) => {
        return await apiRequest('/api/jobs', 'POST', jobData, true);
    },
    
    update: async (jobId, jobData) => {
        return await apiRequest(`/api/jobs/${jobId}`, 'PUT', jobData, true);
    },
    
    delete: async (jobId) => {
        return await apiRequest(`/api/jobs/${jobId}`, 'DELETE', null, true);
    }
};

// ============================================
// APPLICATION API
// ============================================
const ApplicationAPI = {
    submit: async (applicationData) => {
        return await apiRequest('/api/applications', 'POST', applicationData, true);
    },
    
    getMyApplications: async () => {
        return await apiRequest('/api/applications/candidate', 'GET', null, true);
    },
    
    getByJob: async (jobId) => {
        return await apiRequest(`/api/applications/recruiter/${jobId}`, 'GET', null, true);
    },
    
    updateStatus: async (applicationId, statusData) => {
        return await apiRequest(`/api/applications/${applicationId}/status`, 'PUT', statusData, true);
    }
};

// ============================================
// INTERVIEW API
// ============================================
const InterviewAPI = {
    schedule: async (interviewData) => {
        return await apiRequest('/api/interviews/schedule', 'POST', interviewData, true);
    },
    
    getMyInterviews: async () => {
        return await apiRequest('/api/interviews/my', 'GET', null, true);
    }
};

// ============================================
// FEEDBACK API
// ============================================
const FeedbackAPI = {
    submit: async (feedbackData) => {
        return await apiRequest('/api/feedbacks', 'POST', feedbackData, true);
    }
};

// ============================================
// ADMIN API
// ============================================
const AdminAPI = {
    getUsers: async () => {
        return await apiRequest('/api/admin/users', 'GET', null, true);
    },
    
    updateUserRole: async (userId, roleData) => {
        return await apiRequest(`/api/admin/users/${userId}/role`, 'PUT', roleData, true);
    },
    
    getAnalytics: async () => {
        return await apiRequest('/api/analytics/recruitment', 'GET', null, true);
    },
    
    getHealth: async () => {
        return await apiRequest('/api/system/health', 'GET', null, true);
    }
};

// ============================================
// EXPOSE TO GLOBAL SCOPE
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
    setCurrentUser
};

console.log('✅ API Client Loaded Successfully!');
console.log('📍 API Base URL:', API_BASE_URL);