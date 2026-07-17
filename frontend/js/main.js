(function() {
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
  // NOTIFICATIONS
  // ============================================
  function initNotifications() {
    const notifBtn = document.getElementById('notifBtn');
    if (!notifBtn) return;

    let notificationCount = 3;

    notifBtn.addEventListener('click', function() {
      alert(`🔔 You have ${notificationCount} unread notifications:\n\n• Interview scheduled with TechCorp\n• Application viewed by Google\n• New job match: Senior Developer`);
      
      // Clear dot after viewing
      const dot = this.querySelector('.notification-dot');
      if (dot) dot.style.display = 'none';
      notificationCount = 0;
    });
  }

  // ============================================
  // USER PROFILE
  // ============================================
  function initUserAvatar() {
    const avatar = document.getElementById('userAvatar');
    if (!avatar) return;

    avatar.addEventListener('click', function() {
      const actions = ['View Profile', 'Edit Profile', 'Settings', 'Sign Out'];
      const choice = confirm('👤 User Profile\n\nName: Jane Doe\nRole: Senior Software Engineer\nEmail: jane.doe@example.com\n\nClick OK to go to Profile');
      if (choice) {
        window.location.href = 'profile-management.html';
      }
    });
  }

  // ============================================
  // KEYBOARD SHORTCUTS
  // ============================================
  document.addEventListener('keydown', function(e) {
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
  // CONSOLE BRANDING
  // ============================================
  console.log('%c🚀 RecruitAI Platform', 'color: #4f46e5; font-size: 20px; font-weight: 800;');
  console.log('%cVersion 2.0 | Unified UI System', 'color: #6b7280; font-size: 12px;');
  console.log('%c📁 Pages: Dashboard | Jobs | Applications | Profile', 'color: #6b7280; font-size: 12px;');

  // ============================================
  // INIT
  // ============================================
  document.addEventListener('DOMContentLoaded', function() {
    initMobileMenu();
    setActiveNavLink();
    initNotifications();
    initUserAvatar();
    
    console.log('✅ All components initialized successfully!');
  });

})();