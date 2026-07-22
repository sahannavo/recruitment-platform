(function () {
  'use strict';

  // ============================================
  // MOBILE MENU
  // ============================================
  function initMobileMenu() {
    const hamburger = document.getElementById('hamburgerBtn');
    const mobileMenu = document.getElementById('mobileMenu');
    const overlay = document.getElementById('mobileOverlay');

    if (!hamburger || !mobileMenu) return;

    function toggleMenu() {
      mobileMenu.classList.toggle('active');
      overlay.classList.toggle('active');
      document.body.style.overflow = mobileMenu.classList.contains('active') ? 'hidden' : '';
    }

    hamburger.addEventListener('click', toggleMenu);
    overlay.addEventListener('click', toggleMenu);

    // Close on Escape
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && mobileMenu.classList.contains('active')) {
        toggleMenu();
      }
    });

    // Close on link click
    mobileMenu.querySelectorAll('a').forEach(link => {
      link.addEventListener('click', () => {
        if (mobileMenu.classList.contains('active')) {
          toggleMenu();
        }
      });
    });
  }

  // ============================================
  // ACTIVE NAV LINK
  // ============================================
  function setActiveNavLink() {
    const currentPath = window.location.pathname;
    const pageMap = {
      'dashboard': 'dashboard.html',
      'jobs': 'job-search.html',
      'applications': 'my-applications.html',
      'profile': 'profile-management.html'
    };

    document.querySelectorAll('.nav-link, .mobile-link').forEach(link => {
      const page = link.dataset.page;
      if (page && pageMap[page] && currentPath.includes(pageMap[page])) {
        link.classList.add('active');
      }
    });
  }

  // ============================================
  // TOAST NOTIFICATION SYSTEM
  // ============================================
  let toastTimer = null;
  let toastElement = null;

  function getToastElement() {
    if (toastElement) return toastElement;

    toastElement = document.createElement('div');
    toastElement.id = 'toast';
    toastElement.className = 'toast';
    toastElement.style.cssText = `
        position: fixed;
        bottom: 24px;
        left: 50%;
        transform: translateX(-50%) translateY(100px);
        background: var(--text-primary, #111827);
        color: #fff;
        padding: 12px 24px;
        border-radius: 8px;
        font-size: 14px;
        font-weight: 500;
        display: flex;
        align-items: center;
        gap: 10px;
        opacity: 0;
        pointer-events: none;
        transition: opacity 0.3s ease, transform 0.3s ease;
        z-index: 9999;
        box-shadow: 0 10px 25px rgba(0,0,0,0.15);
        max-width: 90%;
    `;
    document.body.appendChild(toastElement);
    return toastElement;
  }

  window.showToast = function (message, type = 'info') {
    const toast = getToastElement();
    const iconMap = {
        success: '✅',
        error: '❌',
        warning: '⚠️',
        info: 'ℹ️'
    };

    const icon = iconMap[type] || 'ℹ️';
    toast.innerHTML = `<span>${icon}</span><span>${message}</span>`;

    const colors = {
        success: 'var(--success, #10b981)',
        error: 'var(--danger, #ef4444)',
        warning: 'var(--warning, #f59e0b)',
        info: 'var(--primary, #4f46e5)'
    };
    toast.style.borderLeft = `4px solid ${colors[type] || colors.info}`;

    toast.style.opacity = '1';
    toast.style.transform = 'translateX(-50%) translateY(0)';
    toast.style.pointerEvents = 'auto';

    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(-50%) translateY(100px)';
        toast.style.pointerEvents = 'none';
    }, 3000);
  };

  document.addEventListener('click', function (e) {
    if (toastElement && toastElement.style.opacity === '1' && toastElement.contains(e.target)) {
        toastElement.style.opacity = '0';
        toastElement.style.transform = 'translateX(-50%) translateY(100px)';
        toastElement.style.pointerEvents = 'none';
        clearTimeout(toastTimer);
    }
  });

  // ============================================
  // NOTIFICATIONS
  // ============================================
  // ============================================
  // USER PROFILE
  // ============================================
  function initUserAvatar() {
    const avatar = document.getElementById('userAvatar');
    if (!avatar) return;

    avatar.addEventListener('click', function () {
      let role = 'Candidate';
      let fullName = 'Jane Doe';
      let email = 'jane.doe@example.com';

      if (window.API && window.API.isAuthenticated()) {
        const user = window.API.getCurrentUser();
        role = window.API.getUserRole();
        fullName = window.API.getUserFullName() || 'User';
        email = user?.email || user?.Email || 'No email';
      }

      const choice = confirm(`👤 User Profile\n\nName: ${fullName}\nRole: ${role}\nEmail: ${email}\n\nClick OK to go to your settings/profile.`);
      if (choice) {
        const path = window.location.pathname;
        if (role === 'Candidate') {
          window.location.href = path.includes('/candidate/') ? 'profile-management.html' : '../candidate/profile-management.html';
        } else if (role === 'Admin' || role === 'SuperAdmin') {
          window.location.href = path.includes('/admin/') ? 'settings.html' : '../admin/settings.html';
        } else {
          alert('Profile configuration is not available for this role.');
        }
      }
    });
  }

  // ============================================
  // KEYBOARD SHORTCUTS
  // ============================================
  document.addEventListener('keydown', function (e) {
    // Ctrl+K or Cmd+K for search
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      const searchInput = document.querySelector('input[type="text"].search-input, input[type="search"]');
      if (searchInput) {
        searchInput.focus();
      }
    }

    // / key for search focus
    if (e.key === '/' && document.activeElement.tagName !== 'INPUT' && document.activeElement.tagName !== 'TEXTAREA') {
      e.preventDefault();
      const searchInput = document.querySelector('input[type="text"].search-input, input[type="search"]');
      if (searchInput) {
        searchInput.focus();
      }
    }
  });

  // ============================================
  // LOGOUT
  // ============================================
  function initLogout() {
    const handleLogout = function (e) {
      e.preventDefault();
      if (confirm('Are you sure you want to sign out?')) {
        if (window.API && window.API.Auth) {
          window.API.Auth.logout();
        }
        window.location.href = '../auth/login.html';
      }
    };

    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) logoutBtn.addEventListener('click', handleLogout);

    const logoutBtnMobile = document.getElementById('logoutBtnMobile');
    if (logoutBtnMobile) logoutBtnMobile.addEventListener('click', handleLogout);

    const logoutBtnDesktop = document.getElementById('logoutBtnDesktop');
    if (logoutBtnDesktop) {
      // Need to attach to parent button if the id is on the icon
      const target = logoutBtnDesktop.closest('button') || logoutBtnDesktop;
      target.addEventListener('click', handleLogout);
    }
  }

  // ============================================
  // CONSOLE BRANDING
  // ============================================
  console.log('%c🚀 RecruitAI Platform', 'color: #4f46e5; font-size: 20px; font-weight: 800;');
  console.log('%cVersion 2.0 | Unified UI System', 'color: #6b7280; font-size: 12px;');
  // ============================================
  // CHATBOT INJECTION
  // ============================================
  function initChatbot() {
    if (!window.API || !window.API.isAuthenticated()) return;
    
    // Inject CSS
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = '../css/chatbot.css';
    document.head.appendChild(link);

    // Inject HTML
    const chatbotHtml = `
      <div class="chatbot-widget">
        <button class="chatbot-btn" id="chatbotBtn" aria-label="Open Chatbot"><i class="fas fa-robot"></i></button>
        <div class="chatbot-window" id="chatbotWindow">
          <div class="chatbot-header">
            <h3><i class="fas fa-robot"></i> AI Assistant</h3>
            <button id="chatbotClose"><i class="fas fa-times"></i></button>
          </div>
          <div class="chatbot-messages" id="chatbotMessages">
            <div class="chat-bubble ai">Hi! I'm your RecruitAI assistant. How can I help you today?</div>
          </div>
          <div class="chatbot-input">
            <input type="text" id="chatbotInput" placeholder="Ask something..." autocomplete="off">
            <button id="chatbotSend"><i class="fas fa-paper-plane"></i></button>
          </div>
        </div>
      </div>
    `;
    document.body.insertAdjacentHTML('beforeend', chatbotHtml);

    // Logic
    const btn = document.getElementById('chatbotBtn');
    const win = document.getElementById('chatbotWindow');
    const closeBtn = document.getElementById('chatbotClose');
    const msgs = document.getElementById('chatbotMessages');
    const input = document.getElementById('chatbotInput');
    const sendBtn = document.getElementById('chatbotSend');

    function toggleChat() {
      win.classList.toggle('open');
      if (win.classList.contains('open')) {
        input.focus();
      }
    }

    btn.addEventListener('click', toggleChat);
    closeBtn.addEventListener('click', toggleChat);

    async function sendMessage() {
      const text = input.value.trim();
      if (!text) return;

      // Add user message
      input.value = '';
      msgs.innerHTML += `<div class="chat-bubble user">${text.replace(/</g, "&lt;")}</div>`;
      msgs.scrollTop = msgs.scrollHeight;

      // Show typing indicator
      const typingId = 'typing-' + Date.now();
      msgs.innerHTML += `
        <div class="typing-indicator" id="${typingId}">
          <span></span><span></span><span></span>
        </div>`;
      msgs.scrollTop = msgs.scrollHeight;

      try {
        const response = await window.API.Chatbot.ask(text);
        document.getElementById(typingId).remove();
        
        let aiReply = response.reply || "Sorry, I couldn't process that.";
        // Simple markdown parsing for bold and newlines
        aiReply = aiReply.replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>");
        aiReply = aiReply.replace(/\n/g, "<br>");
        
        msgs.innerHTML += `<div class="chat-bubble ai">${aiReply}</div>`;
      } catch (e) {
        document.getElementById(typingId).remove();
        msgs.innerHTML += `<div class="chat-bubble ai" style="color: var(--danger)">Error connecting to AI.</div>`;
      }
      msgs.scrollTop = msgs.scrollHeight;
    }

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
      if (e.key === 'Enter') sendMessage();
    });
  }

  // ============================================
  // INIT
  // ============================================
  document.addEventListener('DOMContentLoaded', function () {
    initMobileMenu();
    setActiveNavLink();
    
    initUserAvatar();
    initLogout();
    initChatbot();

    console.log('✅ All components initialized successfully!');
  });

})();