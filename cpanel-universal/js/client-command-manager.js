/**
 * Client Management Operations with Instant Retry
 * Handles all client-related commands with smart retry mechanisms
 */

class ClientCommandManager extends CommandExecutor {
    constructor() {
        super({
            maxRetries: 5,
            retryDelay: 500, // Faster retries for client operations
            timeout: 10000,
            backoffMultiplier: 1.2
        });
        
        this.init();
    }

    init() {
        this.setupFormInterceptors();
        this.setupNavigationInterceptors();
        this.setupDataSeedingRetry();
    }

    /**
     * Setup form submission interceptors with instant retry
     */
    setupFormInterceptors() {
        document.addEventListener('submit', async (event) => {
            const form = event.target;
            
            // Check if it's a client form
            if (this.isClientForm(form)) {
                event.preventDefault();
                await this.handleClientFormSubmission(form);
            }
        });
    }

    /**
     * Setup navigation interceptors
     */
    setupNavigationInterceptors() {
        document.addEventListener('click', async (event) => {
            const link = event.target.closest('a[href]');
            
            if (link && this.isClientLink(link)) {
                event.preventDefault();
                await this.handleClientNavigation(link.href);
            }
        });
    }

    /**
     * Check if form is client-related
     * @param {HTMLFormElement} form 
     * @returns {boolean}
     */
    isClientForm(form) {
        const action = form.action.toLowerCase();
        return action.includes('/clients/') || 
               form.querySelector('input[name="RegistrationNumber"]') !== null ||
               form.id === 'clientForm' ||
               form.id === 'importForm';
    }

    /**
     * Check if link is client-related
     * @param {HTMLAnchorElement} link 
     * @returns {boolean}
     */
    isClientLink(link) {
        const href = link.href.toLowerCase();
        return href.includes('/clients/') ||
               link.classList.contains('client-action') ||
               link.hasAttribute('data-client-action');
    }

    /**
     * Handle client form submission with retry
     * @param {HTMLFormElement} form 
     */
    async handleClientFormSubmission(form) {
        const submitButton = form.querySelector('button[type="submit"]');
        const originalButtonText = submitButton?.innerHTML;

        try {
            // Show loading state
            if (submitButton) {
                submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Se proceseaz?...';
                submitButton.disabled = true;
            }

            // Create progress indicator
            const progressIndicator = this.createProgressIndicator();

            const result = await this.executeWithRetry(async () => {
                const formData = new FormData(form);
                const response = await fetch(form.action, {
                    method: form.method || 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (!response.ok) {
                    throw new Error(`Server error: ${response.status}`);
                }

                return response;
            });

            // Handle successful submission
            this.hideProgressIndicator(progressIndicator);

            if (result.url && result.url !== window.location.href) {
                // Redirect happened
                await this.navigateWithRetry(result.url);
            } else {
                // Same page, possibly with validation errors
                const text = await result.text();
                if (text.includes('alert-danger') || text.includes('validation-summary')) {
                    // Replace current page content to show errors
                    document.open();
                    document.write(text);
                    document.close();
                } else {
                    // Success - redirect to index
                    await this.navigateWithRetry('/Clients');
                }
            }

            this.showSuccessMessage('Opera?iunea a fost finalizat? cu succes!');

        } catch (error) {
            console.error('Client form submission failed:', error);
            this.showErrorMessage(`Eroare la procesarea formularului: ${error.message}`);
        } finally {
            // Restore button
            if (submitButton && originalButtonText) {
                submitButton.innerHTML = originalButtonText;
                submitButton.disabled = false;
            }
        }
    }

    /**
     * Handle client navigation with retry
     * @param {string} url 
     */
    async handleClientNavigation(url) {
        try {
            await this.navigateWithRetry(url, {
                maxRetries: 3,
                retryDelay: 300
            });
        } catch (error) {
            console.error('Client navigation failed:', error);
            this.showErrorMessage(`Nu s-a putut accesa pagina: ${error.message}`);
            
            // Fallback to clients index
            setTimeout(() => {
                window.location.href = '/Clients';
            }, 2000);
        }
    }

    /**
     * Setup data seeding with instant retry
     */
    setupDataSeedingRetry() {
        // Override seed test data function
        window.seedTestDataWithRetry = async () => {
            try {
                const response = await this.executeWithRetry(async () => {
                    const resp = await fetch('/Clients/SeedTestData', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'X-Requested-With': 'XMLHttpRequest',
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                        }
                    });

                    if (!resp.ok) {
                        throw new Error(`Seed data failed: ${resp.status}`);
                    }

                    return resp;
                }, {
                    maxRetries: 5,
                    retryDelay: 1000
                });

                this.showSuccessMessage('100 de clien?i test au fost genera?i cu succes!');
                
                // Refresh page to show new data
                setTimeout(() => {
                    window.location.reload();
                }, 1500);

            } catch (error) {
                console.error('Data seeding failed:', error);
                this.showErrorMessage(`Eroare la generarea datelor test: ${error.message}`);
            }
        };
    }

    /**
     * Handle client deletion with retry
     * @param {number} clientId 
     * @param {string} registrationNumber 
     */
    async deleteClientWithRetry(clientId, registrationNumber) {
        if (!confirm(`E?ti sigur c? vrei s? dezactivezi clientul ${registrationNumber}?`)) {
            return;
        }

        try {
            await this.executeWithRetry(async () => {
                const response = await fetch(`/Clients/Delete/${clientId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    }
                });

                if (!response.ok) {
                    throw new Error(`Delete failed: ${response.status}`);
                }

                return response;
            });

            this.showSuccessMessage(`Clientul ${registrationNumber} a fost dezactivat cu succes!`);
            
            // Remove row from table or refresh page
            const row = document.querySelector(`tr[data-client-id="${clientId}"]`);
            if (row) {
                row.style.transition = 'opacity 0.3s';
                row.style.opacity = '0';
                setTimeout(() => row.remove(), 300);
            } else {
                window.location.reload();
            }

        } catch (error) {
            console.error('Client deletion failed:', error);
            this.showErrorMessage(`Eroare la dezactivarea clientului: ${error.message}`);
        }
    }

    /**
     * Handle Excel import with retry
     * @param {File} file 
     */
    async importExcelWithRetry(file) {
        if (!file) {
            this.showErrorMessage('V? rug?m s? selecta?i un fi?ier Excel.');
            return;
        }

        try {
            const formData = new FormData();
            formData.append('file', file);

            const result = await this.executeWithRetry(async () => {
                const response = await fetch('/Clients/ImportExcel', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                    }
                });

                if (!response.ok) {
                    throw new Error(`Import failed: ${response.status}`);
                }

                return response;
            }, {
                timeout: 30000, // Longer timeout for file uploads
                maxRetries: 3
            });

            this.showSuccessMessage('Fi?ierul a fost importat cu succes!');
            
            // Refresh page to show imported data
            setTimeout(() => {
                window.location.reload();
            }, 2000);

        } catch (error) {
            console.error('Excel import failed:', error);
            this.showErrorMessage(`Eroare la importul fi?ierului: ${error.message}`);
        }
    }

    /**
     * Handle search with instant retry
     * @param {string} searchTerm 
     * @param {Object} filters 
     */
    async searchClientsWithRetry(searchTerm, filters = {}) {
        try {
            const params = new URLSearchParams({
                SearchTerm: searchTerm || '',
                Page: filters.page || 1,
                PageSize: filters.pageSize || 10,
                SortBy: filters.sortBy || 'ExpiryDate',
                SortDirection: filters.sortDirection || 'asc',
                StatusFilter: filters.statusFilter || 'All'
            });

            const response = await this.executeWithRetry(async () => {
                const resp = await fetch(`/Clients?${params.toString()}`, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (!resp.ok) {
                    throw new Error(`Search failed: ${resp.status}`);
                }

                return resp;
            });

            // Update URL without page reload
            const newUrl = `/Clients?${params.toString()}`;
            history.pushState(null, '', newUrl);

            // Update page content
            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const newContent = doc.querySelector('.container');
            
            if (newContent) {
                document.querySelector('.container').innerHTML = newContent.innerHTML;
            }

        } catch (error) {
            console.error('Client search failed:', error);
            this.showErrorMessage(`Eroare la c?utare: ${error.message}`);
        }
    }
}

// Global instance for client operations
window.ClientCommandManager = new ClientCommandManager();

// Global helper functions
window.deleteClientSafely = function(clientId, registrationNumber) {
    return window.ClientCommandManager.deleteClientWithRetry(clientId, registrationNumber);
};

window.importExcelSafely = function(file) {
    return window.ClientCommandManager.importExcelWithRetry(file);
};

window.searchClientsSafely = function(searchTerm, filters) {
    return window.ClientCommandManager.searchClientsWithRetry(searchTerm, filters);
};

// Enhanced page load detection
let pageLoadRetries = 0;
const maxPageLoadRetries = 3;

window.addEventListener('load', function() {
    console.log('? Pagina înc?rcat? cu succes');
    pageLoadRetries = 0;
});

window.addEventListener('error', function(event) {
    pageLoadRetries++;
    
    if (pageLoadRetries <= maxPageLoadRetries) {
        console.warn(`?? Eroare la înc?rcare (încercarea ${pageLoadRetries}). Reîncercare în 2 secunde...`);
        
        setTimeout(() => {
            window.location.reload();
        }, 2000);
    } else {
        console.error('? Pagina nu s-a putut înc?rca dup? mai multe încerc?ri');
        window.ClientCommandManager.showErrorMessage('Pagina nu s-a putut înc?rca complet. V? rug?m s? reînc?rca?i manual.');
    }
});

console.log('? ClientCommandManager initialized with instant retry logic');