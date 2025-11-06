function toggleMobileMenu() {
    const navMenu = document.querySelector('.nav-menu');
    const navUser = document.querySelector('.nav-user');
    
    navMenu.classList.toggle('active');
    if (navUser) {
        navUser.classList.toggle('active');
    }
}

// Highlight active menu based on current URL
document.addEventListener('DOMContentLoaded', function() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-menu a');
    
    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });

    // Handle dropdown clicks on mobile
    if (window.innerWidth <= 768) {
        const dropdowns = document.querySelectorAll('.dropdown');
        dropdowns.forEach(dropdown => {
            dropdown.addEventListener('click', function(e) {
                if (e.target.classList.contains('dropbtn')) {
                    e.preventDefault();
                    this.classList.toggle('active');
                }
            });
        });
    }
});