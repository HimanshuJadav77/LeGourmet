// Restaurant POS & Ordering System JavaScript
// Handles Unobtrusive AJAX, Dynamic Menu Filtering, Persistent Cart Updates, and POS State Transitions

const RestaurantApp = (function () {

    // Initialize Cart Badge count on page load
    function initCartBadge() {
        $.get('/Cart/GetBadgeCount', function (data) {
            if (data && data.count !== undefined) {
                updateBadgeDisplay(data.count);
            }
        });
    }

    function updateBadgeDisplay(count) {
        const badge = $('#cartBadge');
        if (count > 0) {
            badge.text(count).removeClass('d-none');
        } else {
            badge.addClass('d-none');
        }
    }

    // Show dynamic bootstrap toast notification
    function showToast(message, isSuccess = true) {
        let toastEl = $('#liveAppToast');
        if (toastEl.length === 0) {
            $('body').append(`
                <div class="toast-container-fixed">
                    <div id="liveAppToast" class="toast align-items-center text-white border-0 shadow-lg" role="alert" aria-live="assertive" aria-atomic="true">
                        <div class="d-flex">
                            <div class="toast-body d-flex align-items-center gap-2">
                                <i id="toastIcon" class="bi fs-5"></i>
                                <span id="toastMessage"></span>
                            </div>
                            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                        </div>
                    </div>
                </div>
            `);
            toastEl = $('#liveAppToast');
        }

        toastEl.removeClass('bg-success bg-danger bg-dark')
            .addClass(isSuccess ? 'bg-dark' : 'bg-danger');

        $('#toastIcon').removeClass().addClass(isSuccess ? 'bi bi-check-circle-fill text-warning' : 'bi bi-exclamation-octagon-fill text-white');
        $('#toastMessage').text(message);

        const toast = new bootstrap.Toast(toastEl[0], { delay: 3500 });
        toast.show();
    }

    // Add To Cart via Unobtrusive AJAX
    function addToCart(menuItemId, quantity = 1, notes = '') {
        const payload = {
            menuItemId: parseInt(menuItemId),
            quantity: parseInt(quantity),
            notes: notes
        };

        $.ajax({
            url: '/Cart/AddToCartAjax',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    updateBadgeDisplay(res.totalItems);
                    showToast(res.message || 'Added to cart!', true);
                } else {
                    showToast(res.message || 'Error adding item.', false);
                }
            },
            error: function () {
                showToast('Failed to add item to cart. Please try again.', false);
            }
        });
    }

    // Cart Quantity Increment/Decrement via AJAX
    function updateCartQuantity(menuItemId, newQty) {
        const payload = {
            menuItemId: parseInt(menuItemId),
            quantity: parseInt(newQty)
        };

        $.ajax({
            url: '/Cart/UpdateQuantityAjax',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    if (res.quantity <= 0) {
                        $(`#cartRow-${menuItemId}`).fadeOut(300, function () {
                            $(this).remove();
                            if (res.totalItems === 0) {
                                location.reload();
                            }
                        });
                    } else {
                        $(`#itemLineTotal-${menuItemId}`).text('₹' + res.itemLineTotal.toFixed(2));
                        $(`#itemQtyInput-${menuItemId}`).val(res.quantity);
                    }

                    $('#cartSubTotal').text('₹' + res.subTotal.toFixed(2));
                    $('#cartTaxAmount').text('₹' + res.taxAmount.toFixed(2));
                    $('#cartGrandTotal').text('₹' + res.grandTotal.toFixed(2));
                    updateBadgeDisplay(res.totalItems);
                }
            },
            error: function () {
                showToast('Error updating cart quantity.', false);
            }
        });
    }

    // Remove Item from Cart via AJAX
    function removeCartItem(menuItemId) {
        $.post(`/Cart/RemoveFromCartAjax?menuItemId=${menuItemId}`, function (res) {
            if (res.success) {
                $(`#cartRow-${menuItemId}`).fadeOut(300, function () {
                    $(this).remove();
                    if (res.totalItems === 0) {
                        location.reload();
                    }
                });
                $('#cartSubTotal').text('₹' + res.subTotal.toFixed(2));
                $('#cartTaxAmount').text('₹' + res.taxAmount.toFixed(2));
                $('#cartGrandTotal').text('₹' + res.grandTotal.toFixed(2));
                updateBadgeDisplay(res.totalItems);
                showToast('Dish removed from cart.', true);
            }
        });
    }

    // Dynamic Menu Filtering via Unobtrusive AJAX
    function filterMenu() {
        const categoryId = $('.category-chip.active').data('category-id') || '';
        const search = $('#menuSearchInput').val() || '';
        const vegOnly = $('#vegOnlySwitch').is(':checked');
        const sortBy = $('#menuSortSelect').val() || '';

        $('#menuItemsLoader').removeClass('d-none');
        $('#menuItemsContainer').css('opacity', '0.4');

        $.get('/Menu/FilterAjax', {
            categoryId: categoryId,
            search: search,
            vegOnly: vegOnly,
            sortBy: sortBy
        }, function (html) {
            $('#menuItemsContainer').html(html).css('opacity', '1');
            $('#menuItemsLoader').addClass('d-none');
        }).fail(function () {
            $('#menuItemsLoader').addClass('d-none');
            $('#menuItemsContainer').css('opacity', '1');
        });
    }

    // Admin POS Order Status Transition via AJAX
    function updateOrderStatus(orderId, newStatus) {
        $.ajax({
            url: '/Admin/Orders/UpdateStatusAjax',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ orderId: parseInt(orderId), newStatus: newStatus }),
            success: function (res) {
                if (res.success) {
                    showToast(res.message, true);
                    setTimeout(() => location.reload(), 600);
                } else {
                    showToast(res.message || 'Failed to update order status.', false);
                }
            },
            error: function () {
                showToast('Server communication error.', false);
            }
        });
    }

    // Table Availability Check for Reservation via AJAX
    function checkTableAvailability() {
        const date = $('#ReservationDate').val();
        const slot = $('#TimeSlot').val();
        const guests = $('#GuestCount').val() || 2;

        if (!date || !slot) return;

        $('#tableSelectLoader').removeClass('d-none');

        $.get('/Reservation/CheckAvailabilityAjax', {
            date: date,
            timeSlot: slot,
            guestCount: guests
        }, function (tables) {
            $('#tableSelectLoader').addClass('d-none');
            const select = $('#TableId');
            select.empty();

            if (tables.length === 0) {
                select.append('<option value="" disabled selected>No tables available for this time & guest count</option>');
                $('#noTablesAlert').removeClass('d-none');
            } else {
                $('#noTablesAlert').addClass('d-none');
                tables.forEach(t => {
                    select.append(`<option value="${t.tableId}">Table ${t.tableNumber} (${t.location} - Capacity: ${t.capacity} guests)</option>`);
                });
            }
        });
    }

    return {
        initCartBadge,
        addToCart,
        updateCartQuantity,
        removeCartItem,
        filterMenu,
        updateOrderStatus,
        checkTableAvailability,
        showToast
    };
})();

$(document).ready(function () {
    RestaurantApp.initCartBadge();

    // Event delegation for "Add to Cart" buttons
    $(document).on('click', '.btn-add-to-cart', function (e) {
        e.preventDefault();
        const id = $(this).data('item-id');
        const qty = $(this).data('quantity') || 1;
        RestaurantApp.addToCart(id, qty);
    });

    // Menu Filtering Event Listeners
    let debounceTimer;
    $('#menuSearchInput').on('input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(RestaurantApp.filterMenu, 350);
    });

    $('#vegOnlySwitch, #menuSortSelect').on('change', function () {
        RestaurantApp.filterMenu();
    });

    $('.category-chip').on('click', function (e) {
        e.preventDefault();
        $('.category-chip').removeClass('active');
        $(this).addClass('active');
        RestaurantApp.filterMenu();
    });

    // Reservation Availability Event Listeners
    $('#ReservationDate, #TimeSlot, #GuestCount').on('change', function () {
        RestaurantApp.checkTableAvailability();
    });
});
