// Government Portal - Custom JavaScript

// Form Validation
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    // Contact Form Validation (only for non-Umbraco forms)
    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        contactForm.addEventListener('submit', function (event) {
            if (!contactForm.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            } else {
                event.preventDefault();
                handleContactFormSubmit(contactForm);
            }
            contactForm.classList.add('was-validated');
        }, false);
    }

    // Note: Login and Register forms now use server-side validation via Umbraco Surface Controllers
    // JavaScript validation removed to allow proper form submission
});

// Password Validation Function
function validatePassword(password) {
    const minLength = 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    
    return password.length >= minLength && hasUpperCase && hasLowerCase && hasNumber;
}

// Contact Form Submit Handler
function handleContactFormSubmit(form) {
    const formData = new FormData(form);
    const data = Object.fromEntries(formData);
    
    console.log('Contact Form Data:', data);
    
    // Show success message
    showAlert('success', 'Thank you! Your message has been sent successfully. We will get back to you soon.');
    
    // Reset form
    form.reset();
    form.classList.remove('was-validated');
}

// Login Form Submit Handler
function handleLoginFormSubmit(form) {
    const formData = new FormData(form);
    const data = Object.fromEntries(formData);
    
    console.log('Login Form Data:', data);
    
    // In a real application, this would make an API call
    // For demo purposes, we'll show a success message
    showAlert('success', 'Login successful! Redirecting to dashboard...');
    
    // Simulate redirect after 2 seconds
    setTimeout(() => {
        console.log('Redirecting to dashboard...');
    }, 2000);
}

// Register Form Submit Handler
function handleRegisterFormSubmit(form) {
    const formData = new FormData(form);
    const data = Object.fromEntries(formData);
    
    // Remove confirm password from data
    delete data.confirmPassword;
    
    console.log('Register Form Data:', data);
    
    // In a real application, this would make an API call
    // For demo purposes, we'll show a success message
    showAlert('success', 'Account created successfully! Please check your email to verify your account.');
    
    // Reset form
    form.reset();
    form.classList.remove('was-validated');
}

// Alert Helper Function
function showAlert(type, message) {
    const alertContainer = document.createElement('div');
    alertContainer.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3`;
    alertContainer.style.zIndex = '9999';
    alertContainer.style.minWidth = '300px';
    alertContainer.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    document.body.appendChild(alertContainer);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        alertContainer.remove();
    }, 5000);
}

// Smooth Scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const href = this.getAttribute('href');
        if (href !== '#' && href !== '#!' && document.querySelector(href)) {
            e.preventDefault();
            document.querySelector(href).scrollIntoView({
                behavior: 'smooth'
            });
        }
    });
});

// Add animation on scroll
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver(function(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('animate-fade-in-up');
        }
    });
}, observerOptions);

// Observe all cards
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('.card').forEach(card => {
        observer.observe(card);
    });
});
