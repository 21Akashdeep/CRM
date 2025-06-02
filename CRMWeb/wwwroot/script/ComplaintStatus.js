class ComplaintStatus {
    static init() {
        ComplaintStatus.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description', });
                Dropdown.bind({ id: '#ListComplaintId', data: response.data.Complaint, value: 'ComplaintId', text: 'ComplaintNo' });
            }
        });
        $('#btnSearch').on('click', () => {
            ComplaintStatus.getComplaintSchedule({
                onSuccess: (response) => {
                    Table.add({id:'#tableComplaintSchedule', data: response.data});
                }
            });
        });
    }
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'ComplaintStatus/GetViewOption', onSuccess: onSuccess });
    }
    static getComplaintSchedule({ onSuccess }) {
        let obj = {
            FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            ListStatus: $('#ListStatus').val(),
            ListComplaintId: $('#ListComplaintId').val(),
        };
        Data.post({ url: 'ComplaintStatus/GetComplaintSchedule', data: obj, onSuccess: onSuccess });
    }
    static newEntry({ obj }) {        
        Data.post({
            url: 'ComplaintStatus/GetAddOption',
            data: obj,
            onSuccess: (response) => {
                Dropdown.bind({ id: '#Status', data: response.data.Status, value: 'Value', text: 'Description' });
                Modal.open({ id: '#modalComplaintStatus', title: 'Complaint Status / New Entry', action: 'Add', obj: obj });
                Table.add({ id: '#tableComplaintStatus', data: response.data.ComplaintStatus });
            }
        });
    }
}

window.tableComplaintScheduleSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableComplaintScheduleComplaintInfo = (value, obj, index) => {
    return `
        <div>${obj.ComplaintNo}, ${moment(obj.ComplaintDate).format('DD-MMM-YYYY')}</div>
        <div class="fw-bold">Customer</div>
        <div>${obj.CustomerDesc.match(/.{1,50}/g).join('<br>')}</div>
        <div>${obj.CustomerAddress.match(/.{1,50}/g).join('<br>')}</div>
        <div class="fw-bold">Problem</div>
        <div>${obj.Problem.match(/.{1,50}/g).join('<br>')}</div>
    `;
}
window.tableComplaintScheduleStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableComplaintScheduleAction = (value, obj, index) => {
    let actionBtn = `
        <div class="btn-group dropstart">
            <button class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${obj.IsAddStatus ? `
                    <li>
                        <a href="#" class="dropdown-item text-success btn-new-entry" title="New Entry(Add Status)">
                            <span class="fa fa-plus"></span>&nbsp;&nbsp;Add Status
                        </a>
                    </li>` : ``
                }                                              
            </ul>
        </div>
    `;
    return actionBtn;
}
window.tableComplaintScheduleActionEvent = {
    'click .btn-new-entry': (e, value, obj, row) => {
        ComplaintStatus.newEntry({ obj: obj });
    }
}