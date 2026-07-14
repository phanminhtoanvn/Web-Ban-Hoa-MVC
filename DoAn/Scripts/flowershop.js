/**
 * FlowerShop - Main JavaScript File
 * Handles all interactive features for the flower shop website
 */

(function () {
    'use strict';

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        initFavoriteButtons();
        initAddToCart();
        initMobileMenu();
        initSearchBox();
        initNewsletterForm();
        initScrollEffects();
    });

    /**
     * Favorite Button Functionality
     */
    function initFavoriteButtons() {
        const favoriteButtons = document.querySelectorAll('.favorite-btn');

        favoriteButtons.forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                const icon = this.querySelector('i');
                const isFavorite = icon.classList.contains('fas');

                if (isFavorite) {
                    // Remove from favorites
                    icon.classList.remove('fas');
                    icon.classList.add('far');
                    icon.style.color = '';
                    showNotification('Đã xóa khỏi danh sách yêu thích', 'info');
                } else {
                    // Add to favorites
                    icon.classList.remove('far');
                    icon.classList.add('fas');
                    icon.style.color = '#ec4899';
                    showNotification('Đã thêm vào danh sách yêu thích', 'success');
                }

                // Add animation
                btn.style.transform = 'scale(1.2)';
                setTimeout(function () {
                    btn.style.transform = 'scale(1)';
                }, 200);
            });
        });
    }

    /**
     * Add to Cart Functionality
     */
    function initAddToCart() {
        const cartButtons = document.querySelectorAll('.add-to-cart-btn');
        let cartCount = 0;

        cartButtons.forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();

                // Get product details
                const card = this.closest('.product-card');
                const productName = card.querySelector('.product-name').textContent;
                const productPrice = card.querySelector('.current-price').textContent;

                // Update cart count
                cartCount++;
                updateCartBadge(cartCount);

                // Show success message
                showNotification('Đã thêm "' + productName + '" vào giỏ hàng', 'success');

                // Add animation to button
                this.textContent = 'Đã thêm ✓';
                this.style.backgroundColor = '#10b981';

                setTimeout(() => {
                    this.innerHTML = '<i class="fas fa-shopping-cart me-2"></i>Thêm vào giỏ';
                    this.style.backgroundColor = '';
                }, 2000);

                // Animate cart icon
                animateCartIcon();
            });
        });
    }

    /**
     * Update cart badge count
     */
    function updateCartBadge(count) {
        const badge = document.querySelector('.cart-badge');
        if (badge) {
            badge.textContent = count;
            badge.style.transform = 'scale(1.3)';
            setTimeout(function () {
                badge.style.transform = 'scale(1)';
            }, 300);
        }
    }

    /**
     * Animate cart icon
     */
    function animateCartIcon() {
        const cartIcon = document.querySelector('.fa-shopping-cart');
        if (cartIcon) {
            cartIcon.classList.add('fa-bounce');
            setTimeout(function () {
                cartIcon.classList.remove('fa-bounce');
            }, 1000);
        }
    }

    /**
     * Mobile Menu Toggle
     */
    function initMobileMenu() {
        const menuToggle = document.querySelector('[data-bs-toggle="collapse"]');
        const mobileMenu = document.getElementById('mobileMenu');

        if (menuToggle && mobileMenu) {
            menuToggle.addEventListener('click', function () {
                const isExpanded = this.getAttribute('aria-expanded') === 'true';
                this.setAttribute('aria-expanded', !isExpanded);
            });
        }
    }

    /**
     * Search Box Functionality
     */
    function initSearchBox() {
        const searchInputs = document.querySelectorAll('.search-box input');

        searchInputs.forEach(function (input) {
            input.addEventListener('keypress', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const searchTerm = this.value.trim();

                    if (searchTerm) {
                        // Redirect to search results page
                        window.location.href = '/Shop/Search?q=' + encodeURIComponent(searchTerm);
                    } else {
                        showNotification('Vui lòng nhập từ khóa tìm kiếm', 'warning');
                    }
                }
            });

            // Add search icon click handler
            const searchIcon = input.parentElement.querySelector('.search-icon');
            if (searchIcon) {
                searchIcon.style.cursor = 'pointer';
                searchIcon.addEventListener('click', function () {
                    const searchTerm = input.value.trim();
                    if (searchTerm) {
                        window.location.href = '/Shop/Search?q=' + encodeURIComponent(searchTerm);
                    }
                });
            }
        });
    }

    /**
     * Newsletter Form Submission
     */
    function initNewsletterForm() {
        const forms = document.querySelectorAll('.newsletter-form');

        forms.forEach(function (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();

                const emailInput = this.querySelector('.newsletter-input');
                const email = emailInput.value.trim();

                if (!validateEmail(email)) {
                    showNotification('Vui lòng nhập email hợp lệ', 'error');
                    return;
                }

                // Simulate API call
                const submitButton = this.querySelector('.newsletter-btn');
                const originalText = submitButton.textContent;
                submitButton.textContent = 'Đang xử lý...';
                submitButton.disabled = true;

                setTimeout(function () {
                    showNotification('Đăng ký nhận tin thành công! Cảm ơn bạn.', 'success');
                    emailInput.value = '';
                    submitButton.textContent = originalText;
                    submitButton.disabled = false;
                }, 1500);
            });
        });
    }

    /**
     * Scroll Effects
     */
    function initScrollEffects() {
        // Add header shadow on scroll
        let lastScroll = 0;
        const header = document.querySelector('.main-header');

        window.addEventListener('scroll', function () {
            const currentScroll = window.scrollY;

            if (currentScroll > 50) {
                header.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
            } else {
                header.style.boxShadow = '0 1px 3px rgba(0,0,0,0.05)';
            }

            lastScroll = currentScroll;
        });

        // Lazy load images on scroll
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver(function (entries, observer) {
                entries.forEach(function (entry) {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                            observer.unobserve(img);
                        }
                    }
                });
            });

            document.querySelectorAll('img[data-src]').forEach(function (img) {
                imageObserver.observe(img);
            });
        }
    }

    /**
     * Show notification message
     */
    function showNotification(message, type) {
        // Remove existing notifications
        const existingNotification = document.querySelector('.custom-notification');
        if (existingNotification) {
            existingNotification.remove();
        }

        // Create notification element
        const notification = document.createElement('div');
        notification.className = 'custom-notification custom-notification-' + type;
        notification.textContent = message;

        // Add styles
        notification.style.position = 'fixed';
        notification.style.top = '20px';
        notification.style.right = '20px';
        notification.style.padding = '1rem 1.5rem';
        notification.style.borderRadius = '0.5rem';
        notification.style.boxShadow = '0 10px 15px -3px rgba(0, 0, 0, 0.1)';
        notification.style.zIndex = '9999';
        notification.style.maxWidth = '400px';
        notification.style.fontWeight = '500';
        notification.style.animation = 'slideIn 0.3s ease-out';

        // Set colors based on type
        switch (type) {
            case 'success':
                notification.style.backgroundColor = '#10b981';
                notification.style.color = 'white';
                break;
            case 'error':
                notification.style.backgroundColor = '#ef4444';
                notification.style.color = 'white';
                break;
            case 'warning':
                notification.style.backgroundColor = '#f59e0b';
                notification.style.color = 'white';
                break;
            case 'info':
                notification.style.backgroundColor = '#3b82f6';
                notification.style.color = 'white';
                break;
            default:
                notification.style.backgroundColor = '#6b7280';
                notification.style.color = 'white';
        }

        // Add to document
        document.body.appendChild(notification);

        // Remove after 3 seconds
        setTimeout(function () {
            notification.style.animation = 'slideOut 0.3s ease-in';
            setTimeout(function () {
                notification.remove();
            }, 300);
        }, 3000);
    }

    /**
     * Validate email format
     */
    function validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    /**
     * Format currency (VND)
     */
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    // Add CSS animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideIn {
            from {
                transform: translateX(400px);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }

        @keyframes slideOut {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(400px);
                opacity: 0;
            }
        }

        .favorite-btn {
            transition: transform 0.2s ease;
        }

        .cart-badge {
            transition: transform 0.3s ease;
        }
    `;
    document.head.appendChild(style);

    // Export functions for global use if needed
    window.FlowerShop = {
        showNotification: showNotification,
        updateCartBadge: updateCartBadge,
        formatCurrency: formatCurrency
    };

})();
