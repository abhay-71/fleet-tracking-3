/**
 * Fleet Tracking Application - Common JavaScript
 */

// Common DataTables initialization
function initializeDataTable(selector, options = {}) {
    // Default options
    const defaultOptions = {
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        responsive: true,
        language: {
            search: "_INPUT_",
            searchPlaceholder: "Search...",
            emptyTable: "No data available",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            infoEmpty: "Showing 0 to 0 of 0 entries",
            infoFiltered: "(filtered from _MAX_ total entries)",
            lengthMenu: "Show _MENU_ entries",
            loadingRecords: "Loading...",
            processing: "Processing...",
            zeroRecords: "No matching records found"
        }
    };

    // Merge default options with provided options
    const mergedOptions = { ...defaultOptions, ...options };
    
    // Initialize DataTable
    return $(selector).DataTable(mergedOptions);
}

// Format date for display
function formatDate(dateString, format = 'MMM DD, YYYY') {
    if (!dateString) return 'N/A';
    return moment(dateString).format(format);
}

// Format currency for display
function formatCurrency(amount) {
    if (amount === null || amount === undefined) return 'N/A';
    return '$' + parseFloat(amount).toFixed(2);
}

// Generate status badge HTML
function getStatusBadge(status, customClasses = {}) {
    const classes = {
        'active': 'bg-success',
        'inactive': 'bg-secondary',
        'maintenance': 'bg-warning text-dark',
        'scheduled': 'bg-primary',
        'in_progress': 'bg-warning text-dark',
        'completed': 'bg-success',
        'cancelled': 'bg-secondary',
        'overdue': 'bg-danger',
        'critical': 'bg-danger',
        'high': 'bg-warning text-dark',
        'medium': 'bg-info text-dark',
        'low': 'bg-secondary',
        ...customClasses
    };

    const badgeClass = classes[status.toLowerCase()] || 'bg-secondary';
    return `<span class="badge ${badgeClass}">${status}</span>`;
}

// Show confirmation dialog
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Show loading spinner
function showLoading(selector, message = 'Loading...') {
    $(selector).html(`
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading</span>
            </div>
            <p class="mt-2">${message}</p>
        </div>
    `);
}

// Maintenance related functions
const maintenance = {
    // Get priority class based on days until due
    getPriorityClass: function(daysUntilDue) {
        if (daysUntilDue < 0) return 'bg-danger';
        else if (daysUntilDue <= 7) return 'bg-danger';
        else if (daysUntilDue <= 30) return 'bg-warning text-dark';
        else if (daysUntilDue <= 90) return 'bg-info text-dark';
        else return 'bg-secondary';
    },
    
    // Get priority text based on days until due
    getPriorityText: function(daysUntilDue) {
        if (daysUntilDue < 0) return 'Overdue';
        else if (daysUntilDue <= 7) return 'Critical';
        else if (daysUntilDue <= 30) return 'High';
        else if (daysUntilDue <= 90) return 'Medium';
        else return 'Low';
    }
};

// Document ready handler
$(document).ready(function() {
    // Initialize all tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize all popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function(popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // Global AJAX error handler
    $(document).ajaxError(function(event, jqXHR, settings, thrownError) {
        console.error('AJAX Error:', thrownError);
        toastr.error('An error occurred while processing your request. Please try again later.');
    });

    // Initialize all DataTables
    if ($.fn.DataTable) {
        $('.data-table').DataTable({
            responsive: true,
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Search...",
                info: "Showing _START_ to _END_ of _TOTAL_ entries",
                paginate: {
                    first: "First",
                    last: "Last",
                    next: ">",
                    previous: "<"
                }
            },
            pageLength: 10,
            lengthMenu: [[5, 10, 25, 50, -1], [5, 10, 25, 50, "All"]]
        });
    }

    // Add animation to dashboard cards
    $('.dashboard-card').addClass('animate__animated animate__fadeIn');

    // Format dates using moment.js if available
    if (typeof moment !== 'undefined') {
        $('.date-format').each(function () {
            var dateString = $(this).text();
            if (dateString && dateString.trim() !== '') {
                var formattedDate = moment(dateString).format('MMM DD, YYYY');
                $(this).text(formattedDate);
            }
        });

        $('.datetime-format').each(function () {
            var dateString = $(this).text();
            if (dateString && dateString.trim() !== '') {
                var formattedDate = moment(dateString).format('MMM DD, YYYY HH:mm');
                $(this).text(formattedDate);
            }
        });
    }

    // Add confirmation to delete buttons
    $('.btn-delete-confirm').on('click', function (e) {
        if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
            e.preventDefault();
        }
    });

    // Enable file input customization
    $('.custom-file-input').on('change', function () {
        var fileName = $(this).val().split('\\').pop();
        $(this).next('.custom-file-label').html(fileName);
    });

    // Sidebar toggle functionality (if present)
    $('#sidebarToggle').on('click', function (e) {
        e.preventDefault();
        $('body').toggleClass('sb-sidenav-toggled');
    });
}); 