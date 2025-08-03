// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    const darkModeToggle = document.getElementById('darkModeToggle');
    const icon = darkModeToggle.querySelector('i');

    // Check for saved user preference or use system preference
    const prefersDarkScheme = window.matchMedia('(prefers-color-scheme: dark)');
    const currentTheme = localStorage.getItem('theme');

    if (currentTheme === 'dark' || (!currentTheme && prefersDarkScheme.matches)) {
        document.body.classList.add('dark-mode');
        icon.classList.remove('bi-moon-stars-fill');
        icon.classList.add('bi-sun-fill');
    }

    // Toggle dark mode
    darkModeToggle.addEventListener('click', function () {
        const isDarkMode = document.body.classList.toggle('dark-mode');

        if (isDarkMode) {
            icon.classList.remove('bi-moon-stars-fill');
            icon.classList.add('bi-sun-fill');
            localStorage.setItem('theme', 'dark');
        } else {
            icon.classList.remove('bi-sun-fill');
            icon.classList.add('bi-moon-stars-fill');
            localStorage.setItem('theme', 'light');
        }
    });

    // Listen for system preference changes
    prefersDarkScheme.addEventListener('change', e => {
        const newTheme = e.matches ? 'dark' : 'light';
        localStorage.setItem('theme', newTheme);

        if (newTheme === 'dark') {
            document.body.classList.add('dark-mode');
            icon.classList.remove('bi-moon-stars-fill');
            icon.classList.add('bi-sun-fill');
        } else {
            document.body.classList.remove('dark-mode');
            icon.classList.remove('bi-sun-fill');
            icon.classList.add('bi-moon-stars-fill');
        }
    });
});