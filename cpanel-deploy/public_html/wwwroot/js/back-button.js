/**
 * Enhanced Back Button Utilities for Miseda Inspect SRL
 * Provides consistent back navigation across the application
 */

class BackButtonManager {
    constructor() {
        this.init();
    }
    
    init() {
        // Add global keyboard shortcuts
        this.addKeyboardShortcuts();
        
        // Add browser back/forward detection
        this.trackNavigation();
    }
    
    /**
     * Smart back navigation with fallback
     * @param {string} fallbackUrl - URL to navigate to if history is empty
     * @param {boolean} force - Force fallback URL instead of history back
     */
    goBack(fallbackUrl = '/Clients', force = false) {
        if (force) {
            window.location.href = fallbackUrl;
            return;
        }
        
        // Check if we have history and referrer is from our domain
        if (this.canGoBack()) {
            window.history.back();
        } else {
            window.location.href = fallbackUrl;
        }
    }
    
    /**
     * Check if we can safely go back in history
     * @returns {boolean}
     */
    canGoBack() {
        return window.history.length > 1 && 
               document.referrer && 
               document.referrer.indexOf(window.location.origin) === 0 &&
               document.referrer !== window.location.href;
    }
    
    /**
     * Add global keyboard shortcuts
     */
    addKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Alt + Left Arrow = Back
            if (e.altKey && e.key === 'ArrowLeft') {
                e.preventDefault();
                this.goBack();
                this.showShortcutFeedback('Înapoi');
            }
            
            // Alt + Right Arrow = Forward
            if (e.altKey && e.key === 'ArrowRight') {
                e.preventDefault();
                window.history.forward();
                this.showShortcutFeedback('Înainte');
            }
        });
    }
    
    /**
     * Track navigation for analytics/debugging
     */
    trackNavigation() {
        let navigationStart = performance.now();
        
        window.addEventListener('beforeunload', () => {
            console.log(`Page navigation time: ${Math.round(performance.now() - navigationStart)}ms`);
        });
        
        window.addEventListener('popstate', (event) => {
            console.log('Browser navigation detected:', event);
        });
    }
    
    /**
     * Show visual feedback for keyboard shortcuts
     * @param {string} action - Action performed
     */
    showShortcutFeedback(action) {
        // Create or update feedback element
        let feedback = document.getElementById('shortcut-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.id = 'shortcut-feedback';
            feedback.className = 'position-fixed bottom-0 end-0 m-3';
            feedback.style.zIndex = '9999';
            document.body.appendChild(feedback);
        }
        
        feedback.innerHTML = `
            <div class="toast show" role="alert">
                <div class="toast-header">
                    <i class="fas fa-keyboard text-primary me-2"></i>
                    <strong class="me-auto">Navigare</strong>
                </div>
                <div class="toast-body">${action}</div>
            </div>
        `;
        
        // Auto-hide after 2 seconds
        setTimeout(() => {
            if (feedback) {
                feedback.innerHTML = '';
            }
        }, 2000);
    }
    
    /**
     * Create and append a back button to specified element
     * @param {string|Element} target - Target element or selector
     * @param {Object} options - Configuration options
     */
    createBackButton(target, options = {}) {
        const config = {
            text: 'Înapoi',
            icon: 'fas fa-arrow-left',
            className: 'btn btn-outline-secondary',
            fallbackUrl: '/Clients',
            ...options
        };
        
        const targetElement = typeof target === 'string' ? 
            document.querySelector(target) : target;
            
        if (!targetElement) {
            console.warn('Back button target not found:', target);
            return;
        }
        
        const button = document.createElement('button');
        button.type = 'button';
        button.className = config.className;
        button.innerHTML = `<i class="${config.icon}"></i> ${config.text}`;
        
        button.addEventListener('click', () => {
            this.goBack(config.fallbackUrl);
        });
        
        // Add hover effect
        button.addEventListener('mouseenter', function() {
            this.style.transform = 'translateX(-3px)';
            this.style.transition = 'all 0.2s ease';
        });
        
        button.addEventListener('mouseleave', function() {
            this.style.transform = 'translateX(0)';
        });
        
        targetElement.appendChild(button);
        return button;
    }
    
    /**
     * Generate breadcrumb navigation
     * @param {string|Element} target - Target element or selector
     * @param {Object} options - Configuration options
     */
    generateBreadcrumbs(target, options = {}) {
        const config = {
            homeText: 'Acas?',
            homeUrl: '/',
            separator: '/',
            ...options
        };
        
        const targetElement = typeof target === 'string' ? 
            document.querySelector(target) : target;
            
        if (!targetElement) return;
        
        const path = window.location.pathname;
        const segments = path.split('/').filter(s => s);
        
        const breadcrumb = document.createElement('nav');
        breadcrumb.setAttribute('aria-label', 'breadcrumb');
        
        const ol = document.createElement('ol');
        ol.className = 'breadcrumb mb-0';
        
        // Add home
        const homeItem = document.createElement('li');
        homeItem.className = 'breadcrumb-item';
        homeItem.innerHTML = `<a href="${config.homeUrl}"><i class="fas fa-home"></i> ${config.homeText}</a>`;
        ol.appendChild(homeItem);
        
        // Add segments
        let currentPath = '';
        segments.forEach((segment, index) => {
            currentPath += '/' + segment;
            const item = document.createElement('li');
            item.className = 'breadcrumb-item';
            
            const displayName = this.getBreadcrumbName(segment, segments, index);
            
            if (index === segments.length - 1) {
                item.className += ' active';
                item.setAttribute('aria-current', 'page');
                item.textContent = displayName;
            } else {
                item.innerHTML = `<a href="${currentPath}">${displayName}</a>`;
            }
            
            ol.appendChild(item);
        });
        
        breadcrumb.appendChild(ol);
        targetElement.appendChild(breadcrumb);
        
        return breadcrumb;
    }
    
    /**
     * Get display name for breadcrumb segment
     * @param {string} segment - URL segment
     * @param {Array} segments - All segments
     * @param {number} index - Current index
     * @returns {string}
     */
    getBreadcrumbName(segment, segments, index) {
        const names = {
            'clients': 'Clien?i',
            'appointments': 'Program?ri',
            'notifications': 'Notific?ri',
            'admin': 'Administrare',
            'account': 'Cont',
            'test': 'Teste',
            'create': 'Adaug?',
            'edit': 'Editeaz?',
            'details': 'Detalii',
            'dashboard': 'Dashboard',
            'calendar': 'Calendar',
            'users': 'Utilizatori',
            'systeminfo': 'Info Sistem'
        };
        
        // Check if it's an ID (numeric)
        if (!isNaN(segment) && index > 0) {
            const prevSegment = segments[index - 1];
            if (prevSegment === 'clients') {
                // Try to get registration number from page context
                const regNumberElement = document.querySelector('.badge');
                if (regNumberElement) {
                    return regNumberElement.textContent || `Client #${segment}`;
                }
            }
            return `#${segment}`;
        }
        
        return names[segment.toLowerCase()] || 
               segment.charAt(0).toUpperCase() + segment.slice(1);
    }
}

// Global instance
window.BackButtonManager = new BackButtonManager();

// Global helper functions for backward compatibility
window.goBack = function(fallbackUrl) {
    window.BackButtonManager.goBack(fallbackUrl);
};

window.smartGoBack = function(fallbackUrl) {
    window.BackButtonManager.goBack(fallbackUrl);
};

// Initialize on DOM load
document.addEventListener('DOMContentLoaded', function() {
    // Auto-create back buttons for elements with data-back-button attribute
    document.querySelectorAll('[data-back-button]').forEach(element => {
        const options = {
            text: element.dataset.backText || 'Înapoi',
            fallbackUrl: element.dataset.backUrl || '/Clients',
            className: element.dataset.backClass || 'btn btn-outline-secondary'
        };
        
        window.BackButtonManager.createBackButton(element, options);
    });
    
    // Auto-create breadcrumbs for elements with data-breadcrumb attribute
    document.querySelectorAll('[data-breadcrumb]').forEach(element => {
        window.BackButtonManager.generateBreadcrumbs(element);
    });
    
    console.log('BackButtonManager initialized');
});