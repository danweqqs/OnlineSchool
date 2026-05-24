const params = new URLSearchParams(window.location.search);
const teacherId = params.get('id');

if (!teacherId) {
    document.body.innerHTML = '<h2>Teacher ID not specified. Use ?id=1</h2><a href="/">Back</a>';
}

function loadProfile() {
    fetch(`api/teachers/${teacherId}`)
        .then(r => r.json())
        .then(t => {
            document.getElementById('teacher-name').textContent = t.fullName;
            document.getElementById('teacher-email').textContent = t.email || 'N/A';
            document.getElementById('teacher-phone').textContent = t.phone || 'N/A';
            document.getElementById('teacher-subject').textContent = t.subject || 'N/A';
            document.title = t.fullName + ' — Profile';
        });
}

let currentWeekOffset = 0;

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

function formatDate(d) {
    return d.toLocaleDateString();
}

function loadSchedule() {
    fetch(`api/teachers/${teacherId}/schedule`)
        .then(r => r.json())
        .then(data => {
            const range = getWeekRange(currentWeekOffset);
            const filtered = data.filter(l => {
                const d = new Date(l.date);
                return d >= range.start && d <= range.end;
            });

            let header = `<div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:10px;">
                <button onclick="changeWeek(-1)">← Prev</button>
                <span>${formatDate(range.start)} — ${formatDate(range.end)}</span>
                <button onclick="changeWeek(1)">Next →</button>
            </div>`;

            const tbody = document.getElementById('schedule-table');
            tbody.innerHTML = '';
            document.getElementById('week-nav').innerHTML = header;

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
                    <td>${l.student?.fullName || ''}</td>
                    <td class="${statusClass}">${displayStatus}</td>
                </tr>`;
            });
        });
}

function changeWeek(dir) {
    currentWeekOffset += dir;
    loadSchedule();
}

function loadStudents() {
    fetch(`api/teachers/${teacherId}/students`)
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById('students-table');
            tbody.innerHTML = '';
            if (data.length === 0) {
                document.getElementById('no-students').style.display = 'block';
                return;
            }
            document.getElementById('no-students').style.display = 'none';
            data.forEach(s => {
                tbody.innerHTML += `<tr>
                    <td>${s.fullName}</td>
                    <td>${s.email || ''}</td>
                    <td>${s.phone || ''}</td>
                </tr>`;
            });
        });
}

function loadRequests() {
    fetch('api/requests')
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById('requests-table');
            tbody.innerHTML = '';
            const myRequests = data.filter(r => r.teacherId == teacherId);
            myRequests.forEach(r => {
                const date = r.date ? new Date(r.date).toLocaleDateString() : '';
                const statusClass = r.status === 'Approved' ? 'status-active' :
                    r.status === 'Rejected' ? 'status-cancelled' : '';
                tbody.innerHTML += `<tr>
                    <td>${r.type}</td>
                    <td>${r.description || ''}</td>
                    <td>${date}</td>
                    <td class="${statusClass}">${r.status}</td>
                </tr>`;
            });
        });
}

function submitRequest() {
    fetch('api/requests', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            teacherId: parseInt(teacherId),
            type: document.getElementById('request-type').value,
            description: document.getElementById('request-desc').value.trim()
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        document.getElementById('request-desc').value = '';
        loadRequests();
    });
}

if (teacherId) {
    loadProfile();
    loadSchedule();
    loadStudents();
    loadRequests();
}