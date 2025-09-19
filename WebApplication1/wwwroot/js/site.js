// Basic site JavaScript functionality
document.addEventListener('DOMContentLoaded', function() {
    // Initialize any client-side functionality here
    console.log('Site JavaScript loaded successfully');
    
    // Auto-hide success messages after 5 seconds
    const successAlerts = document.querySelectorAll('.alert-success');
    successAlerts.forEach(function(alert) {
        setTimeout(function() {
            alert.style.display = 'none';
        }, 5000);
    });
});