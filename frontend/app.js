/**
 * RecruitAI - Multi-Page Application Controller
 * Handles navigation, page transitions, tab switching, and UI interactions.
 */

(function () {
    'use strict';

    // ===== State =====
    let currentPage = 'profile';
    let currentSettingsTab = 'general';

    // ===== DOM References =====
    const loadingScreen = document.getElementById('loading-screen');
    const app = document.getElementById('app');
    const toast = document.getElementById('toast');
    const toastMessage = document.getElementById('toast-message');
    const toastIcon = document.getElementById('toast-icon');

    // ===== Initialization =====
    function init() {
        // Simulate loading
        setTimeout(() => {
            if (loadingScreen) {
                loadingScreen.style.opacity = '0';
                loadingScreen.style.pointerEvents = 'none';
            }
            if (app) {
                app.style.opacity = '1';
            }
            setTimeout(() => {
                if (loadingScreen) loadingScreen.remove();
            }, 500);
        }, 1200);

        // Check hash for initial page
        const hash = window.location.hash.replace('#', '') || 'profile';
        navigateTo(hash, false);

        // Set up event listeners
        setupSettingsTabs();
        setupProfileSubNav();
        setupSaveButtons();
        setupToastDismiss();

        // Handle browser back/forward
        window.addEventListener('hashchange', () => {
            const newPage = window.location.hash.replace('#', '') || 'profile';
            navigateTo(newPage, false);
        });
    }

    // ===== Navigation =====
    window.navigateTo = function (page, updateHash = true) {
        // Validate page
        const validPages = ['profile', 'analytics', 'settings'];
        if (!validPages.includes(page)) page = 'profile';

        // Don't navigate if already on this page
        if (page === currentPage) return;

        // Hide all pages
        const pages = document.querySelectorAll('.page');
        const currentPageEl = document.getElementById(`page-${currentPage}`);

        // Add exit animation to current page
        if (currentPageEl && !currentPageEl.classList.contains('hidden')) {
            currentPageEl.classList.add('page-exit');
            setTimeout(() => {
                currentPageEl.classList.add('hidden');
                currentPageEl.classList.remove('page-exit');
                showPage(page);
            }, 200);
        } else {
            // First load - no exit animation needed
            pages.forEach(p => p.classList.add('hidden'));
            showPage(page);
        }

        // Update state
        currentPage = page;

        // Update URL hash
        if (updateHash) {
            window.location.hash = page;
        }

        // Update nav links
        updateNavLinks(page);

        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    function showPage(page) {
        const pageEl = document.getElementById(`page-${page}`);
        if (pageEl) {
            pageEl.classList.remove('hidden');
            // Re-trigger animations for bar charts when analytics page is shown
            if (page === 'analytics') {
                triggerAnalyticsAnimations();
            }
        }
    }

    function updateNavLinks(page) {
        const navLinks = document.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            const linkPage = link.getAttribute('data-page');
            if (linkPage === page) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });
    }

    // ===== Settings Tabs =====
    function setupSettingsTabs() {
        const tabs = document.querySelectorAll('.settings-tab');
        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                const tabName = tab.getAttribute('data-tab');
                switchSettingsTab(tabName);
            });
        });
    }

    function switchSettingsTab(tabName) {
        if (tabName === currentSettingsTab) return;

        // Update tab buttons
        const tabs = document.querySelectorAll('.settings-tab');
        tabs.forEach(tab => {
            if (tab.getAttribute('data-tab') === tabName) {
                tab.classList.add('text-primary', 'border-primary', 'font-body-strong');
                tab.classList.remove('text-on-surface-variant', 'border-transparent', 'font-body-base');
            } else {
                tab.classList.remove('text-primary', 'border-primary', 'font-body-strong');
                tab.classList.add('text-on-surface-variant', 'border-transparent', 'font-body-base');
            }
        });

        // Update tab content
        const contents = document.querySelectorAll('.settings-tab-content');
        contents.forEach(content => content.classList.add('hidden'));

        const activeContent = document.getElementById(`tab-${tabName}`);
        if (activeContent) {
            activeContent.classList.remove('hidden');
            activeContent.style.animation = 'none';
            activeContent.offsetHeight; // Trigger reflow
            activeContent.style.animation = 'sectionFadeIn 0.4s ease-out';
        }

        currentSettingsTab = tabName;
    }

    // ===== Profile Sub-Navigation =====
    function setupProfileSubNav() {
        const subNavLinks = document.querySelectorAll('.profile-sub-nav');
        subNavLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                const section = link.getAttribute('data-section');
                if (section === 'settings') return; // Let navigateTo handle this

                e.preventDefault();

                // Update active state
                subNavLinks.forEach(l => {
                    l.classList.remove('active', 'text-primary', 'font-body-strong', 'bg-primary-fixed');
                    l.classList.add('text-on-surface-variant', 'font-body-base');
                });
                link.classList.add('active', 'text-primary', 'font-body-strong', 'bg-primary-fixed');
                link.classList.remove('text-on-surface-variant', 'font-body-base');

                // Scroll to section
                const sectionEl = document.getElementById(`section-${section}`);
                if (sectionEl) {
                    sectionEl.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            });
        });
    }

    // ===== Save Buttons =====
    function setupSaveButtons() {
        // Profile save
        const btnSaveProfile = document.getElementById('btn-save-profile');
        if (btnSaveProfile) {
            btnSaveProfile.addEventListener('click', () => {
                showToast('Profile changes saved successfully!', 'check_circle');
                pulseButton(btnSaveProfile);
            });
        }

        // Discard
        const btnDiscard = document.getElementById('btn-discard');
        if (btnDiscard) {
            btnDiscard.addEventListener('click', () => {
                showToast('Changes discarded.', 'undo');
            });
        }

        // General settings save
        const btnSaveGeneral = document.getElementById('btn-save-general');
        if (btnSaveGeneral) {
            btnSaveGeneral.addEventListener('click', () => {
                showToast('Company settings saved!', 'check_circle');
                pulseButton(btnSaveGeneral);
            });
        }

        // Export report
        const btnExport = document.getElementById('btn-export');
        if (btnExport) {
            btnExport.addEventListener('click', () => {
                showToast('Report exported as PDF!', 'download');
            });
        }

        // Apply dates
        const btnApplyDates = document.getElementById('btn-apply-dates');
        if (btnApplyDates) {
            btnApplyDates.addEventListener('click', () => {
                showToast('Date range applied. Refreshing data...', 'calendar_today');
                triggerAnalyticsAnimations();
            });
        }

        // All other save buttons (AI Weights, Notifications)
        document.querySelectorAll('#tab-ai-weights button, #tab-notifications button').forEach(btn => {
            if (btn.textContent.includes('Save')) {
                btn.addEventListener('click', () => {
                    showToast('Settings saved successfully!', 'check_circle');
                    pulseButton(btn);
                });
            }
        });
    }

    // ===== Toast System =====
    let toastTimeout = null;

    function showToast(message, icon = 'check_circle') {
        if (toastTimeout) clearTimeout(toastTimeout);

        toastMessage.textContent = message;
        toastIcon.textContent = icon;
        toast.classList.add('toast-visible');

        toastTimeout = setTimeout(() => {
            toast.classList.remove('toast-visible');
        }, 3000);
    }

    function setupToastDismiss() {
        if (toast) {
            toast.addEventListener('click', () => {
                toast.classList.remove('toast-visible');
                if (toastTimeout) clearTimeout(toastTimeout);
            });
        }
    }

    // ===== Button Pulse Effect =====
    function pulseButton(btn) {
        btn.style.transform = 'scale(0.95)';
        setTimeout(() => {
            btn.style.transform = 'scale(1)';
        }, 150);
    }

    // ===== Analytics Animations =====
    function triggerAnalyticsAnimations() {
        // Re-trigger bar chart animations
        const bars = document.querySelectorAll('.animate-bar-grow');
        bars.forEach(bar => {
            const width = bar.style.width;
            bar.style.width = '0';
            bar.style.animation = 'none';
            bar.offsetHeight; // Trigger reflow
            bar.style.animation = '';
            bar.style.width = width;
        });

        // Re-trigger line chart animation
        const chartLine = document.querySelector('.animate-chart-line');
        if (chartLine) {
            chartLine.style.animation = 'none';
            chartLine.offsetHeight;
            chartLine.style.animation = '';
        }

        const chartFill = document.querySelector('.animate-chart-fill');
        if (chartFill) {
            chartFill.style.animation = 'none';
            chartFill.offsetHeight;
            chartFill.style.animation = '';
        }
    }

    // ===== Notification Button =====
    const notifBtn = document.getElementById('notification-btn');
    if (notifBtn) {
        notifBtn.addEventListener('click', () => {
            showToast('No new notifications', 'notifications_active');
            // Remove the pulse dot
            const dot = notifBtn.querySelector('.bg-error');
            if (dot) dot.remove();
        });
    }

    // ===== CV Dropzone Interaction =====
    const cvDropzone = document.getElementById('cv-dropzone');
    if (cvDropzone) {
        cvDropzone.addEventListener('click', () => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = '.pdf,.docx,.txt';
            input.onchange = (e) => {
                const file = e.target.files[0];
                if (file) {
                    cvDropzone.innerHTML = `
                        <div class="flex items-center gap-3">
                            <span class="material-symbols-outlined text-[32px] text-secondary">description</span>
                            <div class="text-left">
                                <p class="text-body-strong font-body-strong text-on-surface">${file.name}</p>
                                <p class="text-label-sm font-label-sm text-on-surface-variant">${(file.size / 1024).toFixed(1)} KB</p>
                            </div>
                            <span class="material-symbols-outlined text-secondary text-[20px] ml-auto">check_circle</span>
                        </div>
                    `;
                    cvDropzone.classList.remove('border-dashed');
                    cvDropzone.classList.add('border-secondary', 'bg-secondary-fixed/10');
                    showToast('CV uploaded successfully! AI is parsing...', 'auto_awesome');
                }
            };
            input.click();
        });

        // Drag and drop
        cvDropzone.addEventListener('dragover', (e) => {
            e.preventDefault();
            cvDropzone.classList.add('border-primary', 'bg-primary-fixed/20');
        });

        cvDropzone.addEventListener('dragleave', () => {
            cvDropzone.classList.remove('border-primary', 'bg-primary-fixed/20');
        });

        cvDropzone.addEventListener('drop', (e) => {
            e.preventDefault();
            cvDropzone.classList.remove('border-primary', 'bg-primary-fixed/20');
            const file = e.dataTransfer.files[0];
            if (file) {
                showToast(`"${file.name}" uploaded!`, 'check_circle');
            }
        });
    }

    // ===== Logo Dropzone Interaction =====
    const logoDropzone = document.getElementById('logo-dropzone');
    if (logoDropzone) {
        logoDropzone.addEventListener('click', () => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = '.png,.jpg,.jpeg,.gif';
            input.onchange = (e) => {
                const file = e.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = (event) => {
                        logoDropzone.innerHTML = `
                            <img src="${event.target.result}" alt="Company Logo" class="w-24 h-24 object-contain rounded-lg" />
                            <p class="text-label-sm text-on-surface-variant mt-2">${file.name}</p>
                        `;
                    };
                    reader.readAsDataURL(file);
                    showToast('Logo uploaded successfully!', 'check_circle');
                }
            };
            input.click();
        });
    }

    // ===== Chart Tooltip & Interactivity =====
    function setupChartInteractions() {
        const chartArea = document.getElementById('tth-chart-area');
        const tooltip = document.getElementById('tth-tooltip');
        const tooltipLabel = document.getElementById('tth-tooltip-label');
        const tooltipValue = document.getElementById('tth-tooltip-value');
        const dots = document.querySelectorAll('.chart-dot');

        if (!chartArea || !tooltip) return;

        // Track mouse for vertical guide line
        chartArea.addEventListener('mousemove', (e) => {
            const rect = chartArea.getBoundingClientRect();
            const x = e.clientX - rect.left;
            chartArea.style.setProperty('--hover-x', x + 'px');
        });

        // Tooltip on data point hover
        dots.forEach(dot => {
            dot.addEventListener('mouseenter', (e) => {
                const value = dot.getAttribute('data-value');
                const label = dot.getAttribute('data-label');
                tooltipLabel.textContent = label;
                tooltipValue.textContent = value + ' days';

                // Position tooltip relative to chart area
                const chartRect = chartArea.getBoundingClientRect();
                const svg = document.getElementById('tth-svg');
                const svgRect = svg.getBoundingClientRect();

                // Convert SVG coordinates to pixel coordinates
                const cx = parseFloat(dot.getAttribute('cx'));
                const cy = parseFloat(dot.getAttribute('cy'));
                const svgViewBox = svg.viewBox.baseVal;
                const xPx = (cx / svgViewBox.width) * svgRect.width;
                const yPx = (cy / svgViewBox.height) * svgRect.height;

                tooltip.style.left = xPx + 'px';
                tooltip.style.top = (yPx - 8) + 'px';
                tooltip.style.opacity = '1';

                // Enlarge dot
                dot.setAttribute('r', '6');
                dot.style.filter = 'url(#dotGlow)';
            });

            dot.addEventListener('mouseleave', () => {
                tooltip.style.opacity = '0';
                dot.setAttribute('r', '4');
                dot.style.filter = '';
            });
        });
    }

    // ===== Skills Section =====
    function setupSkills() {
        const btnAddSkill = document.getElementById('btn-add-skill');
        const skillForm = document.getElementById('skill-add-form');
        const btnCancel = document.getElementById('btn-cancel-skill');
        const btnConfirm = document.getElementById('btn-confirm-skill');
        const inputName = document.getElementById('input-skill-name');
        const inputCategory = document.getElementById('input-skill-category');
        const inputLevel = document.getElementById('input-skill-level');

        if (!btnAddSkill || !skillForm) return;

        // Toggle form
        btnAddSkill.addEventListener('click', () => {
            skillForm.classList.toggle('hidden');
            if (!skillForm.classList.contains('hidden')) {
                skillForm.style.animation = 'none';
                skillForm.offsetHeight;
                skillForm.style.animation = '';
                inputName.focus();
            }
        });

        // Cancel
        btnCancel.addEventListener('click', () => {
            skillForm.classList.add('hidden');
            inputName.value = '';
        });

        // Add skill
        btnConfirm.addEventListener('click', () => {
            const name = inputName.value.trim();
            if (!name) {
                inputName.style.borderColor = '#ba1a1a';
                setTimeout(() => { inputName.style.borderColor = ''; }, 1500);
                return;
            }

            const category = inputCategory.value;
            const level = inputLevel.value;
            const container = document.getElementById(`skills-${category}`);
            if (!container) return;

            // Level config
            const levelConfig = {
                expert: { label: 'Expert', width: '90%', colorClass: 'bg-primary', badgeBg: 'bg-primary/10', badgeText: 'text-primary', badgeBorder: 'border-primary/20' },
                advanced: { label: 'Advanced', width: '75%', colorClass: 'bg-secondary', badgeBg: 'bg-secondary/10', badgeText: 'text-secondary', badgeBorder: 'border-secondary/20' },
                intermediate: { label: 'Intermediate', width: '55%', colorClass: 'bg-tertiary', badgeBg: 'bg-tertiary/10', badgeText: 'text-tertiary', badgeBorder: 'border-tertiary/20' },
                beginner: { label: 'Beginner', width: '30%', colorClass: 'bg-outline', badgeBg: 'bg-outline/10', badgeText: 'text-outline', badgeBorder: 'border-outline/20' }
            };

            const cfg = levelConfig[level];

            // Create skill element
            const skillEl = document.createElement('div');
            skillEl.className = 'skill-item skill-item-new group flex items-center gap-3';
            skillEl.setAttribute('data-skill', name);
            skillEl.innerHTML = `
                <div class="flex-1 min-w-0">
                    <div class="flex items-center justify-between mb-1">
                        <span class="text-body-base font-body-base text-on-surface truncate">${name}</span>
                        <span class="skill-level-badge text-[10px] font-semibold px-2 py-0.5 rounded-full ${cfg.badgeBg} ${cfg.badgeText} border ${cfg.badgeBorder}">${cfg.label}</span>
                    </div>
                    <div class="w-full h-1.5 bg-surface-container-high rounded-full overflow-hidden">
                        <div class="skill-bar h-full rounded-full ${cfg.colorClass} transition-all duration-700" style="width: ${cfg.width}"></div>
                    </div>
                </div>
                <button class="skill-remove-btn text-outline hover:text-error transition-all opacity-0 group-hover:opacity-100 p-0.5 rounded hover:bg-error-container" title="Remove skill">
                    <span class="material-symbols-outlined text-[16px]">close</span>
                </button>
            `;

            // Wire remove button
            skillEl.querySelector('.skill-remove-btn').addEventListener('click', () => removeSkill(skillEl));

            container.appendChild(skillEl);

            // Update count
            updateCategoryCount(category);

            // Reset form
            inputName.value = '';
            skillForm.classList.add('hidden');

            showToast(`"${name}" added to ${inputCategory.options[inputCategory.selectedIndex].text}!`, 'check_circle');
        });

        // Wire up existing remove buttons
        document.querySelectorAll('.skill-remove-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const skillItem = btn.closest('.skill-item');
                if (skillItem) removeSkill(skillItem);
            });
        });

        // Enter key in input
        inputName.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') btnConfirm.click();
        });
    }

    function removeSkill(skillEl) {
        const skillName = skillEl.getAttribute('data-skill');
        const categoryCard = skillEl.closest('.skill-category-card');
        const category = categoryCard ? categoryCard.getAttribute('data-category') : null;

        skillEl.classList.add('skill-item-removing');
        setTimeout(() => {
            skillEl.remove();
            if (category) updateCategoryCount(category);
            showToast(`"${skillName}" removed.`, 'delete');
        }, 300);
    }

    function updateCategoryCount(category) {
        const container = document.getElementById(`skills-${category}`);
        const card = document.querySelector(`.skill-category-card[data-category="${category}"]`);
        if (!container || !card) return;

        const count = container.querySelectorAll('.skill-item').length;
        const countEl = card.querySelector('.text-label-sm.ml-auto');
        if (countEl) {
            countEl.textContent = `${count} skill${count !== 1 ? 's' : ''}`;
        }
    }

    // ===== Initialize on DOM Ready =====
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => { init(); setupChartInteractions(); setupSkills(); });
    } else {
        init();
        setupChartInteractions();
        setupSkills();
    }
})();
