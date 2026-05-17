const API = {
    subjects: 'api/subjects',
    teachers: 'api/teachers',
    students: 'api/students',
    lessons: 'api/lessons',
    requests: 'api/requests'
};

function showTab(name) {
    document.querySelectorAll('.tab-content').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    document.getElementById(name).classList.add('active');
    event.target.classList.add('active');

    if (name === 'subjects') getSubjects();
    if (name === 'teachers') { getTeachers(); loadSubjectsInto('teacher-subject'); loadSubjectsInto('edit-teacher-subject'); }
    if (name === 'students') getStudents();
    if (name === 'lessons') { getLessons(); loadLessonSelects(); }
    if (name === 'requests') { getRequests(); loadTeacherSelect(); }
}

function hideEdit(id) {
    document.getElementById(id).style.display = 'none';
}

function loadSubjectsInto(selectId) {
    fetch(API.subjects).then(r => r.json()).then(data => {
        const sel = document.getElementById(selectId);
        sel.innerHTML = '<option value="">Subject</option>';
        data.forEach(s => sel.innerHTML += `<option value="${s.id}">${s.name}</option>`);
    });
}

function getSubjects() {
    fetch(API.subjects)
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById('subjects-table');
            tbody.innerHTML = '';
            data.forEach(s => {
                tbody.innerHTML += `<tr>
                    <td>${s.name}</td>
                    <td>${s.description || ''}</td>
                    <td>
                        <button onclick="editSubject(${s.id}, '${s.name}', '${s.description || ''}')">Edit</button>
                        <button onclick="deleteSubject(${s.id})">Delete</button>
                    </td>
                </tr>`;
            });
        });
}

function addSubject() {
    const name = document.getElementById('subject-name').value.trim();
    const desc = document.getElementById('subject-desc').value.trim();
    fetch(API.subjects, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, description: desc })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        document.getElementById('subject-name').value = '';
        document.getElementById('subject-desc').value = '';
        getSubjects();
    });
}

function editSubject(id, name, desc) {
    document.getElementById('edit-subject').style.display = 'block';
    document.getElementById('edit-subject-id').value = id;
    document.getElementById('edit-subject-name').value = name;
    document.getElementById('edit-subject-desc').value = desc;
}

function updateSubject() {
    const id = document.getElementById('edit-subject-id').value;
    fetch(`${API.subjects}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            id: parseInt(id),
            name: document.getElementById('edit-subject-name').value.trim(),
            description: document.getElementById('edit-subject-desc').value.trim()
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        hideEdit('edit-subject');
        getSubjects();
    });
}

function deleteSubject(id) {
    if (!confirm('Delete this subject?')) return;
    fetch(`${API.subjects}/${id}`, { method: 'DELETE' })
        .then(r => {
            if (!r.ok) return r.json().then(e => alert(e.message));
            getSubjects();
        });
}

function getTeachers() {
    fetch(API.teachers)
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById('teachers-table');
            tbody.innerHTML = '';
            data.forEach(t => {
                tbody.innerHTML += `<tr>
                    <td>${t.fullName}</td>
                    <td>${t.email || ''}</td>
                    <td>${t.phone || ''}</td>
                    <td>${t.subject || ''}</td>
                    <td>
                        <button onclick="editTeacher(${t.id}, '${t.fullName}', '${t.email || ''}', '${t.phone || ''}', ${t.subjectId || 'null'})">Edit</button>
                        <button onclick="showSchedule(${t.id}, '${t.fullName}', this)">Schedule</button>
                        <button onclick="deleteTeacher(${t.id})">Delete</button>
                    </td>
                </tr>`;
            });
        });
}

function addTeacher() {
    const subjectVal = document.getElementById('teacher-subject').value;
    fetch(API.teachers, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            fullName: document.getElementById('teacher-name').value.trim(),
            email: document.getElementById('teacher-email').value.trim(),
            phone: document.getElementById('teacher-phone').value.trim(),
            subjectId: subjectVal ? parseInt(subjectVal) : null
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        document.getElementById('teacher-name').value = '';
        document.getElementById('teacher-email').value = '';
        document.getElementById('teacher-phone').value = '';
        document.getElementById('teacher-subject').value = '';
        getTeachers();
    });
}

function editTeacher(id, name, email, phone, subjectId) {
    document.getElementById('edit-teacher').style.display = 'block';
    document.getElementById('edit-teacher-id').value = id;
    document.getElementById('edit-teacher-name').value = name;
    document.getElementById('edit-teacher-email').value = email;
    document.getElementById('edit-teacher-phone').value = phone;
    loadSubjectsInto('edit-teacher-subject');
    setTimeout(() => {
        document.getElementById('edit-teacher-subject').value = subjectId || '';
    }, 300);
}

function updateTeacher() {
    const id = document.getElementById('edit-teacher-id').value;
    const subjectVal = document.getElementById('edit-teacher-subject').value;
    fetch(`${API.teachers}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            id: parseInt(id),
            fullName: document.getElementById('edit-teacher-name').value.trim(),
            email: document.getElementById('edit-teacher-email').value.trim(),
            phone: document.getElementById('edit-teacher-phone').value.trim(),
            subjectId: subjectVal ? parseInt(subjectVal) : null
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        hideEdit('edit-teacher');
        getTeachers();
    });
}

function deleteTeacher(id) {
    if (!confirm('Delete this teacher?')) return;
    fetch(`${API.teachers}/${id}`, { method: 'DELETE' })
        .then(r => {
            if (!r.ok) return r.json().then(e => alert(e.message));
            getTeachers();
        });
}

function getStudents() {
    fetch(API.students)
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById('students-table');
            tbody.innerHTML = '';
            data.forEach(s => {
                const date = s.dateRegistered ? new Date(s.dateRegistered).toLocaleDateString() : '';
                tbody.innerHTML += `<tr>
                    <td>${s.fullName}</td>
                    <td>${s.email || ''}</td>
                    <td>${s.phone || ''}</td>
                    <td>${date}</td>
                    <td>
                        <button onclick="editStudent(${s.id}, '${s.fullName}', '${s.email || ''}', '${s.phone || ''}')">Edit</button>
                        <button onclick="deleteStudent(${s.id})">Delete</button>
                    </td>
                </tr>`;
            });
        });
}

function addStudent() {
    fetch(API.students, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            fullName: document.getElementById('student-name').value.trim(),
            email: document.getElementById('student-email').value.trim(),
            phone: document.getElementById('student-phone').value.trim()
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        document.getElementById('student-name').value = '';
        document.getElementById('student-email').value = '';
        document.getElementById('student-phone').value = '';
        getStudents();
    });
}

function editStudent(id, name, email, phone) {
    document.getElementById('edit-student').style.display = 'block';
    document.getElementById('edit-student-id').value = id;
    document.getElementById('edit-student-name').value = name;
    document.getElementById('edit-student-email').value = email;
    document.getElementById('edit-student-phone').value = phone;
}

function updateStudent() {
    const id = document.getElementById('edit-student-id').value;
    fetch(`${API.students}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            id: parseInt(id),
            fullName: document.getElementById('edit-student-name').value.trim(),
            email: document.getElementById('edit-student-email').value.trim(),
            phone: document.getElementById('edit-student-phone').value.trim(),
            dateRegistered: new Date().toISOString()
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        hideEdit('edit-student');
        getStudents();
    });
}

function deleteStudent(id) {
    if (!confirm('Delete this student?')) return;
    fetch(`${API.students}/${id}`, { method: 'DELETE' })
        .then(r => {
            if (!r.ok) return r.json().then(e => alert(e.message));
            getStudents();
        });
}

function loadLessonSelects() {
    loadSubjectsInto('lesson-subject');
    document.getElementById('lesson-teacher').innerHTML = '<option value="">Teacher</option>';
    fetch(API.students).then(r => r.json()).then(data => {
        const sel = document.getElementById('lesson-student');
        sel.innerHTML = '<option value="">Student</option>';
        data.forEach(s => sel.innerHTML += `<option value="${s.id}">${s.fullName}</option>`);
    });
}

function loadTeachersBySubject() {
    const subjectId = document.getElementById('lesson-subject').value;
    const sel = document.getElementById('lesson-teacher');
    sel.innerHTML = '<option value="">Teacher</option>';
    if (!subjectId) return;
    fetch(API.teachers).then(r => r.json()).then(data => {
        const filtered = data.filter(t => t.subjectId == subjectId);
        if (filtered.length === 0) {
            sel.innerHTML = '<option value="">No teachers for this subject</option>';
            return;
        }
        filtered.forEach(t => sel.innerHTML += `<option value="${t.id}">${t.fullName}</option>`);
    });
}

function getLessons() {
    fetch(API.lessons)
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById('lessons-table');
            tbody.innerHTML = '';
            data.forEach(l => {
                const date = l.date ? new Date(l.date).toLocaleDateString() : '';
                const time = l.time || '';
                let displayStatus = l.status;
                const lessonEnd = new Date(l.date);
                const timeParts = (l.time || '0:0:0').split(':');
                lessonEnd.setHours(parseInt(timeParts[0]), parseInt(timeParts[1]) + (l.durationMinutes || 45));
                if (l.status === 'Scheduled' && lessonEnd < new Date()) displayStatus = 'Completed';
                const statusClass = displayStatus === 'Cancelled' ? 'status-cancelled' : displayStatus === 'Completed' ? 'status-completed' : 'status-active';
                tbody.innerHTML += `<tr>
                    <td>${date}</td>
                    <td>${time}</td>
                    <td>${l.durationMinutes} min</td>
                    <td>${l.subject || ''}</td>
                    <td>${l.teacher || ''}</td>
                    <td>${l.student || ''}</td>
                    <td class="${statusClass}">${displayStatus}</td>
                    <td>
                        ${displayStatus !== 'Cancelled' && displayStatus !== 'Completed' ? `<button onclick="cancelLesson(${l.id})">Cancel</button>` : ''}
                        <button onclick="deleteLesson(${l.id})">Delete</button>
                    </td>
                </tr>`;
            });
        });
}

function addLesson() {
    const selectedDate = document.getElementById('lesson-date').value;
    const selectedTime = document.getElementById('lesson-time').value;
    const lessonDateTime = new Date(selectedDate + 'T' + selectedTime);
    if (lessonDateTime < new Date()) {
        if (!confirm('This lesson is in the past. Continue?')) return;
    }
    fetch(API.lessons, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            subjectId: parseInt(document.getElementById('lesson-subject').value),
            teacherId: parseInt(document.getElementById('lesson-teacher').value),
            studentId: parseInt(document.getElementById('lesson-student').value),
            date: selectedDate,
            time: selectedTime + ':00',
            durationMinutes: parseInt(document.getElementById('lesson-duration').value) || 45
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        getLessons();
    });
}

function cancelLesson(id) {
    if (!confirm('Cancel this lesson?')) return;
    fetch(`${API.lessons}/${id}/cancel`, { method: 'PATCH' })
        .then(r => {
            if (!r.ok) return r.json().then(e => alert(e.message));
            getLessons();
        });
}

function deleteLesson(id) {
    if (!confirm('Delete this lesson?')) return;
    fetch(`${API.lessons}/${id}`, { method: 'DELETE' })
        .then(() => getLessons());
}

function loadTeacherSelect() {
    fetch(API.teachers).then(r => r.json()).then(data => {
        const sel = document.getElementById('request-teacher');
        sel.innerHTML = '<option value="">Teacher</option>';
        data.forEach(t => sel.innerHTML += `<option value="${t.id}">${t.fullName}</option>`);
    });
}

function getRequests() {
    fetch(API.requests)
        .then(r => r.json())
        .then(data => {
            const tbody = document.getElementById('requests-table');
            tbody.innerHTML = '';
            data.forEach(r => {
                const date = r.date ? new Date(r.date).toLocaleDateString() : '';
                const statusClass = r.status === 'Approved' ? 'status-active' :
                    r.status === 'Rejected' ? 'status-cancelled' : '';
                tbody.innerHTML += `<tr>
                    <td>${r.teacher || ''}</td>
                    <td>${r.type}</td>
                    <td>${r.description || ''}</td>
                    <td>${date}</td>
                    <td class="${statusClass}">${r.status}</td>
                    <td>
                        ${r.status === 'Pending' ? `
                            <button onclick="approveRequest(${r.id})">Approve</button>
                            <button onclick="rejectRequest(${r.id})">Reject</button>
                        ` : ''}
                        <button onclick="deleteRequest(${r.id})">Delete</button>
                    </td>
                </tr>`;
            });
        });
}

function addRequest() {
    fetch(API.requests, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            teacherId: parseInt(document.getElementById('request-teacher').value),
            type: document.getElementById('request-type').value,
            description: document.getElementById('request-desc').value.trim()
        })
    }).then(r => {
        if (!r.ok) return r.json().then(e => alert(e.message));
        document.getElementById('request-desc').value = '';
        getRequests();
    });
}

function approveRequest(id) {
    fetch(`${API.requests}/${id}/approve`, { method: 'PATCH' })
        .then(r => {
            if (!r.ok) return r.json().then(e => alert(e.message));
            getRequests();
        });
}

function rejectRequest(id) {
    fetch(`${API.requests}/${id}/reject`, { method: 'PATCH' })
        .then(r => {
            if (!r.ok) return r.json().then(e => alert(e.message));
            getRequests();
        });
}

function deleteRequest(id) {
    if (!confirm('Delete this request?')) return;
    fetch(`${API.requests}/${id}`, { method: 'DELETE' })
        .then(() => getRequests());
}

function showSchedule(teacherId, teacherName, btn) {
    const existingRow = document.getElementById('schedule-row');
    if (existingRow) existingRow.remove();

    const tr = btn.closest('tr');
    const newRow = document.createElement('tr');
    newRow.id = 'schedule-row';
    const td = document.createElement('td');
    td.colSpan = 5;
    td.className = 'edit-form';
    td.innerHTML = 'Loading...';
    newRow.appendChild(td);
    tr.after(newRow);

    fetch(`${API.teachers}/${teacherId}/schedule`)
        .then(r => r.json())
        .then(data => {
            let html = `<strong>${teacherName} — Schedule</strong>`;
            if (data.length === 0) {
                html += '<p>No lessons</p>';
            } else {
                html += '<table><thead><tr><th>Date</th><th>Time</th><th>Duration</th><th>Subject</th><th>Student</th><th>Status</th></tr></thead><tbody>';
                data.forEach(l => {
                    const date = l.date ? new Date(l.date).toLocaleDateString() : '';
                    let displayStatus = l.status;
                    const lessonEnd = new Date(l.date);
                    const timeParts = (l.time || '0:0:0').split(':');
                    lessonEnd.setHours(parseInt(timeParts[0]), parseInt(timeParts[1]) + (l.durationMinutes || 45));
                    if (l.status === 'Scheduled' && lessonEnd < new Date()) displayStatus = 'Completed';
                    const statusClass = displayStatus === 'Cancelled' ? 'status-cancelled' : displayStatus === 'Completed' ? 'status-completed' : 'status-active';
                    html += `<tr>
                        <td>${date}</td>
                        <td>${l.time || ''}</td>
                        <td>${l.durationMinutes} min</td>
                        <td>${l.subject?.name || ''}</td>
                        <td>${l.student?.fullName || ''}</td>
                        <td class="${statusClass}">${displayStatus}</td>
                    </tr>`;
                });
                html += '</tbody></table>';
            }
            html += '<button onclick="document.getElementById(\'schedule-row\').remove()">Close</button>';
            td.innerHTML = html;
        });
}

function closeSchedule() {
    document.getElementById('schedule-view').style.display = 'none';
}

getSubjects();