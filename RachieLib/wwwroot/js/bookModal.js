// Book modal functions
function openAddBookModal() {
    document.getElementById('addBookModal').classList.remove('hidden');
    document.getElementById('title').focus();
}

function closeAddBookModal() {
    document.getElementById('addBookModal').classList.add('hidden');
}

// Initialize modal event listeners
document.addEventListener('DOMContentLoaded', function() {
    const modal = document.getElementById('addBookModal');
    if (modal) {
        // Close modal when clicking outside
        modal.addEventListener('click', function(e) {
            if (e.target === this) {
                closeAddBookModal();
            }
        });

        // Close modal on escape key
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && !modal.classList.contains('hidden')) {
                closeAddBookModal();
            }
        });
    }
});
