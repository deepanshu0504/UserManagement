// Theme Toggle with localStorage Persistence
(function() {
    'use strict';

    const STORAGE_KEY = 'user-theme-preference';
    const THEME_DARK = 'theme-dark';
    const THEME_LIGHT = 'theme-light';

    // Get current theme from localStorage or default to dark
    function getCurrentTheme() {
        return localStorage.getItem(STORAGE_KEY) || THEME_DARK;
    }

    // Apply theme to body element
    function applyTheme(theme) {
        document.body.classList.remove(THEME_DARK, THEME_LIGHT);
        document.body.classList.add(theme);
        updateToggleIcon(theme);
    }

    // Update toggle button icon
    function updateToggleIcon(theme) {
        const toggleBtn = document.getElementById('theme-toggle');
        if (!toggleBtn) return;
        
        if (theme === THEME_DARK) {
            toggleBtn.innerHTML = '☀️';
            toggleBtn.setAttribute('aria-label', 'Switch to light mode');
        } else {
            toggleBtn.innerHTML = '🌙';
            toggleBtn.setAttribute('aria-label', 'Switch to dark mode');
        }
    }

    // Toggle between themes
    function toggleTheme() {
        const currentTheme = getCurrentTheme();
        const newTheme = currentTheme === THEME_DARK ? THEME_LIGHT : THEME_DARK;
        
        localStorage.setItem(STORAGE_KEY, newTheme);
        applyTheme(newTheme);
    }

    // Initialize theme on page load
    function init() {
        const savedTheme = getCurrentTheme();
        applyTheme(savedTheme);

        // Attach event listener to toggle button
        const toggleBtn = document.getElementById('theme-toggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', toggleTheme);
        }
    }

    // Run on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Expose toggle function globally for inline usage if needed
    window.toggleTheme = toggleTheme;
})();
