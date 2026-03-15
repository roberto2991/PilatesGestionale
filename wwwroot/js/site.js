// ─── Sidebar Toggle ───
const sidebar = document.getElementById('sidebar');
const toggle  = document.getElementById('sidebarToggle');

if (toggle && sidebar) {
  toggle.addEventListener('click', () => {
    sidebar.classList.toggle('open');
  });
}

// ─── Auto-dismiss alerts after 4s ───
document.querySelectorAll('.alert').forEach(alert => {
  setTimeout(() => {
    const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
    if (bsAlert) bsAlert.close();
  }, 4000);
});

// ─── Confirm delete dialogs ───
document.querySelectorAll('[data-confirm]').forEach(btn => {
  btn.addEventListener('click', e => {
    const msg = btn.getAttribute('data-confirm') || 'Sei sicuro di voler procedere?';
    if (!confirm(msg)) e.preventDefault();
  });
});
