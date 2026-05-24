const params = new URLSearchParams(window.location.search);
const studentId = params.get('id');

if (!studentId) {
    document.body.innerHTML = '<h2>Student ID not specified. Use ?id=1</h2><a href="/">Back</a>';
}

let currentWeekOffset = 0;

function loadProfile() {
    fetch(`api/students/${studentId}`)
        .then(r => r.json())
        .then(s => {
            document.getElementById('student-name').textContent = s.fullName;
            document.getElementById('student-email').textContent = s.email || 'N/A';
            document.getElementById('student-phone').textContent = s.phone || 'N/A';
            document.getElementById('student-date').textContent = s.dateRegistered ? new Date(s.dateRegistered).toLocaleDateString() : '';
            updateBalanceDisplay(s.balance);
            document.title = s.fullName + ' — Profile';
        });
}

function updateBalanceDisplay(balance) {
    const el = document.getElementById('student-balance');
    el.textContent = (balance || 0) + ' ₴';
    el.className = 'balance-box ' + (balance > 0 ? 'balance-positive' : 'balance-zero');
}

function topUp() {
    const amount = parseFloat(document.getElementById('topup-amount').value);
    if (!amount || amount <= 0) return alert('Enter a valid amount');

    fetch(`api/students/${studentId}/topup`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(amount)
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        return r.json();
    }).then(data => {
        if (data) {
            updateBalanceDisplay(data.balance);
            document.getElementById('topup-amount').value = '';
        }
    });
}

function getWeekRange(offset) {
    const now = new Date();
    const monday = new Date(now);
    monday.setDate(now.getDate() - now.getDay() + 1 + offset * 7);
    monday.setHours(0, 0, 0, 0);
    const sunday = new Date(monday);
    sunday.setDate(monday.getDate() + 6);
    sunday.setHours(23, 59, 59, 999);
    return { start: monday, end: sunday };
}

function loadSchedule() {
    fetch(`api/students/${studentId}/schedule`)
        .then(r => r.json())
        .then(data => {
            const range = getWeekRange(currentWeekOffset);
            const filtered = data.filter(l => {
                const d = new Date(l.date);
                return d >= range.start && d <= range.end;
            });

            document.getElementById('week-nav').innerHTML = `<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:10px;">
                <button onclick="changeWeek(-1)">← Prev</button>
                <span>${range.start.toLocaleDateString()} — ${range.end.toLocaleDateString()}</span>
                <button onclick="changeWeek(1)">Next →</button>
            </div>`;

            const tbody = document.getElementById('schedule-table');
            tbody.innerHTML = '';

            if (filtered.length === 0) {
                document.getElementById('no-lessons').style.display = 'block';
                return;
            }
            document.getElementById('no-lessons').style.display = 'none';
            filtered.forEach(l => {
                const date = l.date ? new Date(l.date).toLocaleDateString() : '';
                let displayStatus = l.status;
                const lessonEnd = new Date(l.date);
                const timeParts = (l.time || '0:0:0').split(':');
                lessonEnd.setHours(parseInt(timeParts[0]), parseInt(timeParts[1]) + (l.durationMinutes || 45));
                if (l.status === 'Scheduled' && lessonEnd < new Date()) displayStatus = 'Completed';
                const statusClass = displayStatus === 'Cancelled' ? 'status-cancelled' : displayStatus === 'Completed' ? 'status-completed' : 'status-active';
                tbody.innerHTML += `<tr>
                    <td>${date}</td>
                    <td>${l.time || ''}</td>
                    <td>${l.durationMinutes} min</td>
                    <td>${l.subject?.name || ''}</td>
                    <td>${l.teacher?.fullName || ''}</td>
                    <td class="${statusClass}">${displayStatus}</td>
                </tr>`;
            });
        });
}

function changeWeek(dir) {
    currentWeekOffset += dir;
    loadSchedule();
}

function loadStats() {
    fetch(`api/students/${studentId}/schedule`)
        .then(r => r.json())
        .then(data => {
            let completed = 0, cancelled = 0, scheduled = 0;
            data.forEach(l => {
                const lessonEnd = new Date(l.date);
                const timeParts = (l.time || '0:0:0').split(':');
                lessonEnd.setHours(parseInt(timeParts[0]), parseInt(timeParts[1]) + (l.durationMinutes || 45));
                if (l.status === 'Cancelled') cancelled++;
                else if (lessonEnd < new Date()) completed++;
                else scheduled++;
            });
            document.getElementById('stats').innerHTML = `
                <span>Total: <strong>${data.length}</strong></span>
                <span class="status-completed">Completed: <strong>${completed}</strong></span>
                <span class="status-active">Scheduled: <strong>${scheduled}</strong></span>
                <span class="status-cancelled">Cancelled: <strong>${cancelled}</strong></span>
            `;
        });
}

if (studentId) {
    loadProfile();
    loadSchedule();
    loadStats();
}