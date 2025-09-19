/**
 * Command Execution Utility with Retry Logic
 * Handles instant loading failures and implements smart retry mechanisms
 */

class CommandExecutor {
    constructor(options = {}) {
        this.maxRetries = options.maxRetries || 3;
        this.retryDelay = options.retryDelay || 1000; // 1 second
        this.timeout = options.timeout || 5000; // 5 seconds
        this.backoffMultiplier = options.backoffMultiplier || 1.5;
    }

    /**
     * Execute command with retry logic
     * @param {Function} command - Command to execute
     * @param {Object} options - Execution options
     * @returns {Promise}
     */
    async executeWithRetry(command, options = {}) {
        const config = {
            ...this,
            ...options
        };

        let lastError;
        let delay = config.retryDelay;

        for (let attempt = 1; attempt <= config.maxRetries; attempt++) {
            try {
                console.log(`Încercare ${attempt}/${config.maxRetries}...`);
                
                // Execute with timeout
                const result = await this.executeWithTimeout(command, config.timeout);
                
                console.log(`? Comand? executat? cu succes la încercarea ${attempt}`);
                return result;
                
            } catch (error) {
                lastError = error;
                console.warn(`? Încercarea ${attempt} e?uat?:`, error.message);
                
                // Don't wait after the last attempt
                if (attempt < config.maxRetries) {
                    console.log(`? Reîncercare în ${delay}ms...`);
                    await this.sleep(delay);
                    delay *= config.backoffMultiplier; // Exponential backoff
                }
            }
        }

        throw new Error(`Comanda a e?uat dup? ${config.maxRetries} încerc?ri. Ultima eroare: ${lastError.message}`);
    }

    /**
     * Execute command with timeout
     * @param {Function} command - Command to execute
     * @param {number} timeout - Timeout in milliseconds
     * @returns {Promise}
     */
    executeWithTimeout(command, timeout) {
        return new Promise((resolve, reject) => {
            const timeoutId = setTimeout(() => {
                reject(new Error(`Timeout: Comanda nu s-a înc?rcat în ${timeout}ms`));
            }, timeout);

            Promise.resolve(command())
                .then(result => {
                    clearTimeout(timeoutId);
                    resolve(result);
                })
                .catch(error => {
                    clearTimeout(timeoutId);
                    reject(error);
                });
        });
    }

    /**
     * Sleep utility
     * @param {number} ms - Milliseconds to sleep
     * @returns {Promise}
     */
    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    /**
     * Execute HTTP request with retry
     * @param {string} url - URL to request
     * @param {Object} options - Fetch options
     * @returns {Promise}
     */
    async fetchWithRetry(url, options = {}) {
        return this.executeWithRetry(async () => {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), this.timeout);

            try {
                const response = await fetch(url, {
                    ...options,
                    signal: controller.signal
                });

                clearTimeout(timeoutId);

                if (!response.ok) {
                    throw new Error(`HTTP Error: ${response.status} ${response.statusText}`);
                }

                return response;
            } catch (error) {
                clearTimeout(timeoutId);
                throw error;
            }
        });
    }

    /**
     * Execute form submission with retry
     * @param {HTMLFormElement} form - Form to submit
     * @param {Object} options - Submission options
     * @returns {Promise}
     */
    async submitFormWithRetry(form, options = {}) {
        return this.executeWithRetry(async () => {
            const formData = new FormData(form);
            const url = form.action || window.location.href;
            const method = form.method || 'POST';

            const response = await this.fetchWithRetry(url, {
                method: method.toUpperCase(),
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            return response;
        }, options);
    }

    /**
     * Execute navigation with retry
     * @param {string} url - URL to navigate to
     * @param {Object} options - Navigation options
     * @returns {Promise}
     */
    async navigateWithRetry(url, options = {}) {
        return this.executeWithRetry(async () => {
            // Check if URL is accessible first
            await this.fetchWithRetry(url, { method: 'HEAD' });
            
            // Navigate
            if (options.replace) {
                window.location.replace(url);
            } else {
                window.location.href = url;
            }
            
            return true;
        }, options);
    }

    /**
     * Execute AJAX request with retry and progress feedback
     * @param {Object} config - Request configuration
     * @returns {Promise}
     */
    async ajaxWithRetry(config) {
        const showProgress = config.showProgress !== false;
        let progressElement;

        if (showProgress) {
            progressElement = this.createProgressIndicator();
        }

        try {
            const result = await this.executeWithRetry(async () => {
                const response = await this.fetchWithRetry(config.url, {
                    method: config.method || 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        ...config.headers
                    },
                    body: config.data ? JSON.stringify(config.data) : undefined
                });

                return response.json();
            }, config.retryOptions);

            if (progressElement) {
                this.hideProgressIndicator(progressElement);
            }

            return result;

        } catch (error) {
            if (progressElement) {
                this.hideProgressIndicator(progressElement);
                this.showErrorMessage(`Opera?iunea a e?uat: ${error.message}`);
            }
            throw error;
        }
    }

    /**
     * Create progress indicator
     * @returns {HTMLElement}
     */
    createProgressIndicator() {
        const existing = document.getElementById('command-progress');
        if (existing) return existing;

        const progress = document.createElement('div');
        progress.id = 'command-progress';
        progress.className = 'position-fixed top-50 start-50 translate-middle';
        progress.style.zIndex = '9999';
        progress.innerHTML = `
            <div class="card shadow">
                <div class="card-body text-center">
                    <div class="spinner-border text-primary mb-3" role="status">
                        <span class="visually-hidden">Se încarc?...</span>
                    </div>
                    <h6>Se execut? comanda...</h6>
                    <p class="text-muted small mb-0">V? rug?m s? a?tepta?i</p>
                </div>
            </div>
        `;

        document.body.appendChild(progress);
        return progress;
    }

    /**
     * Hide progress indicator
     * @param {HTMLElement} progressElement
     */
    hideProgressIndicator(progressElement) {
        if (progressElement && progressElement.parentNode) {
            progressElement.parentNode.removeChild(progressElement);
        }
    }

    /**
     * Show error message
     * @param {string} message
     */
    showErrorMessage(message) {
        // Create error toast
        const toast = document.createElement('div');
        toast.className = 'position-fixed top-0 end-0 m-3';
        toast.style.zIndex = '9999';
        toast.innerHTML = `
            <div class="toast show" role="alert">
                <div class="toast-header bg-danger text-white">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong class="me-auto">Eroare</strong>
                    <button type="button" class="btn-close btn-close-white" onclick="this.closest('.toast').remove()"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        document.body.appendChild(toast);

        // Auto-hide after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 5000);
    }

    /**
     * Show success message
     * @param {string} message
     */
    showSuccessMessage(message) {
        const toast = document.createElement('div');
        toast.className = 'position-fixed top-0 end-0 m-3';
        toast.style.zIndex = '9999';
        toast.innerHTML = `
            <div class="toast show" role="alert">
                <div class="toast-header bg-success text-white">
                    <i class="fas fa-check-circle me-2"></i>
                    <strong class="me-auto">Succes</strong>
                    <button type="button" class="btn-close btn-close-white" onclick="this.closest('.toast').remove()"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        document.body.appendChild(toast);

        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 3000);
    }
}

// Global instance
window.CommandExecutor = new CommandExecutor({
    maxRetries: 3,
    retryDelay: 1000,
    timeout: 8000,
    backoffMultiplier: 1.5
});

// Enhanced global functions with retry logic
window.executeWithRetry = async function(command, options = {}) {
    return window.CommandExecutor.executeWithRetry(command, options);
};

window.fetchWithRetry = async function(url, options = {}) {
    return window.CommandExecutor.fetchWithRetry(url, options);
};

window.navigateWithRetry = async function(url, options = {}) {
    return window.CommandExecutor.navigateWithRetry(url, options);
};

// Enhanced form submission
window.submitFormSafely = async function(form, options = {}) {
    try {
        const response = await window.CommandExecutor.submitFormWithRetry(form, options);
        window.CommandExecutor.showSuccessMessage('Formular trimis cu succes!');
        return response;
    } catch (error) {
        window.CommandExecutor.showErrorMessage(`Eroare la trimiterea formularului: ${error.message}`);
        throw error;
    }
};

// Enhanced navigation with loading state
window.navigateSafely = async function(url, options = {}) {
    try {
        // Show loading state
        const button = event?.target;
        if (button) {
            const originalText = button.innerHTML;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Se încarc?...';
            button.disabled = true;

            setTimeout(() => {
                if (button) {
                    button.innerHTML = originalText;
                    button.disabled = false;
                }
            }, 10000); // Reset after 10 seconds as failsafe
        }

        await window.CommandExecutor.navigateWithRetry(url, options);
    } catch (error) {
        window.CommandExecutor.showErrorMessage(`Eroare la navigare: ${error.message}`);
        throw error;
    }
};

// Enhanced AJAX with retry
window.ajaxWithRetry = async function(config) {
    return window.CommandExecutor.ajaxWithRetry(config);
};

// Auto-retry failed requests
let failedRequests = [];

window.addEventListener('error', function(event) {
    if (event.target.tagName === 'SCRIPT' || event.target.tagName === 'LINK') {
        console.warn('Resursa nu s-a înc?rcat:', event.target.src || event.target.href);
        
        // Retry loading the resource
        setTimeout(() => {
            if (event.target.tagName === 'SCRIPT') {
                const newScript = document.createElement('script');
                newScript.src = event.target.src;
                newScript.async = true;
                document.head.appendChild(newScript);
            } else if (event.target.tagName === 'LINK') {
                const newLink = document.createElement('link');
                newLink.href = event.target.href;
                newLink.rel = 'stylesheet';
                document.head.appendChild(newLink);
            }
        }, 2000);
    }
});

// Network status monitoring
window.addEventListener('online', function() {
    console.log('?? Conexiunea la internet a fost restabilit?');
    window.CommandExecutor.showSuccessMessage('Conexiunea la internet a fost restabilit?');
    
    // Retry failed requests
    if (failedRequests.length > 0) {
        console.log(`Reîncercare ${failedRequests.length} cereri e?uate...`);
        failedRequests.forEach(request => {
            setTimeout(() => request(), Math.random() * 2000);
        });
        failedRequests = [];
    }
});

window.addEventListener('offline', function() {
    console.log('?? Conexiunea la internet a fost întrerupt?');
    window.CommandExecutor.showErrorMessage('Conexiunea la internet a fost întrerupt?');
});

console.log('? CommandExecutor initialized with retry logic');