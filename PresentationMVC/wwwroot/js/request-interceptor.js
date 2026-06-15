// Request Interceptor - إضافة Token إلى جميع الـ requests
// يتأكد من أن جميع الـ HTTP requests تحتوي على Authorization header مع الـ JWT Token
// و يضيف Token كـ cookie أيضاً

(function() {
    'use strict';

    // Store original fetch function
    const originalFetch = window.fetch;

    // Override fetch to add JWT token to all requests
    window.fetch = function(...args) {
        let [resource, config] = args;

        // Ensure config is an object
        config = config || {};

        // Get token from TokenManager
        const token = TokenManager?.getToken?.();

        if (token) {
            // Initialize headers if not present
            if (!config.headers) {
                config.headers = {};
            }

            // Add authorization header
            config.headers['Authorization'] = `Bearer ${token}`;
            config.headers['X-Access-Token'] = token;
        }

        // Call original fetch with updated config
        return originalFetch.apply(window, [resource, config]);
    };

    // Also intercept XMLHttpRequest
    const originalOpen = XMLHttpRequest.prototype.open;
    XMLHttpRequest.prototype.open = function(method, url, ...rest) {
        this._url = url;
        this._method = method;
        return originalOpen.apply(this, [method, url, ...rest]);
    };

    const originalSend = XMLHttpRequest.prototype.send;
    XMLHttpRequest.prototype.send = function(...args) {
        const token = TokenManager?.getToken?.();
        if (token) {
            this.setRequestHeader('Authorization', `Bearer ${token}`);
            this.setRequestHeader('X-Access-Token', token);
        }
        return originalSend.apply(this, args);
    };

    // استخدم navigation interceptor لإضافة Token إلى Form submissions
    // عبر إضافة hidden input بـ CSRF token و Token
    document.addEventListener('DOMContentLoaded', function() {
        // intercept form submissions
        document.addEventListener('submit', function(e) {
            const form = e.target;
            if (!form.method || form.method.toUpperCase() !== 'POST') {
                return;
            }

            const token = TokenManager?.getToken?.();
            if (token && !form.querySelector('input[name="X-Access-Token"]')) {
                // إضيف Token كـ hidden input
                const tokenInput = document.createElement('input');
                tokenInput.type = 'hidden';
                tokenInput.name = 'X-Access-Token';
                tokenInput.value = token;
                form.appendChild(tokenInput);
            }
        }, true); // capture phase
    });

    console.log('Request interceptor initialized - Token will be added to all requests');
})();