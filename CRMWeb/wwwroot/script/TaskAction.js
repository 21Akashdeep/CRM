class TaskAction {
    static init() {
        TaskAction.getViewOption({
            onSuccess: (response) => {
                Dropdown.bind({ id: '#ListStatus', data: response.data.Status, value: 'Value', text: 'Description', });
               
            }
        });

        $('#btnSearch').on('click', () => {
            TaskAction.getTask({
                onSuccess: (response) => {
                    Table.add({ id: '#tableTask', data: response.data });
                }
            });
        });
        $('#Status').on('change', () => {
            if ($('#Status').val() == 4) {
                $('.send-otp').removeClass('hide');
                $('#OTP').addClass('required');
            }
            else {
                $('.send-otp').addClass('hide');
                $('#OTP').removeClass('required');
            }
        });

        $('#btnSendOtp').on('click', () => {
            if (Field.isNullOrEmpty($('#TaskId').val())) {
                Message.error({ statusText: "Task No. empty" });
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formTaskAction" });
            Data.post({
                url: 'TaskAction/SendOtp',
                data: obj,
                onSuccess: (response) => {
                    Message.show(response);
                    if (response.status == Message.Type.success) {
                        TaskAction.startOtpTimer({ durationInSeconds: 30, elementId: "#OtpTimer" });

                    }
                }
            });
        });

        $('#btnSave').on('click', () => {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            let obj = Data.serializeToObject({ formId: "#formTaskAction" });

            
            
            TaskAction.add({ obj: obj });
        });

        $('#TaskAction_TaskItemId').on('change', () => {

            let taskitemid = $('#TaskAction_TaskItemId').val();

            let obj = {
                 
                TaskItemId:taskitemid
            };

            Data.post({
                url: 'TaskAction/GetTaskAction',
                data: obj,
                onSuccess: (response) => {
                    Table.add({
                        id: '#tableTaskAction',
                        data: response.data,
                        selectPick: true
                    });
                }
            });

        });

    }
    
    static getViewOption({ onSuccess }) {
        Data.get({ url: 'TaskAction/GetViewOption', onSuccess: onSuccess });
    }
    static getTask({ onSuccess }) {
        let obj = {
            //FromDate: DateTime.json($('#DateRange').val().split('|')[0]),
            //ToDate: DateTime.json($('#DateRange').val().split('|')[1]),
            
            ListStatus: $('#ListStatus').val(),           
        };
        Data.post({ url: 'TaskAction/GetTask', data: obj, onSuccess: onSuccess });
    }
    static newEntry({ obj }) {
        Data.post({
            url: 'TaskAction/GetAddOption',
            data: obj,
            onSuccess: (response) => {
                Dropdown.bind({ id: '#Status', data: response.data.Status, value: 'Value', text: 'Description' });
                Dropdown.bind({ id: '#TaskAction_TaskItemId', data: response.data.TaskItem, value: 'Id', text: 'Description' });
                Modal.open({ id: '#modalTaskAction', title: 'Task Status / Add', action: 'Add', obj: obj });
                $('.send-otp').addClass('hide');
                TaskAction.stopOtpTimer({ elementId: "#OtpTimer" });
                Table.add({ id: '#tableTaskAction', data: response.data.TaskAction });
                $('#Remarks').val('')
            }
        });
    }
    static otpInterval;
    static startOtpTimer({ durationInSeconds, elementId }) {
        let seconds = durationInSeconds;
        const timerElement = document.querySelector(elementId);
        timerElement.innerHTML = `You can request a new OTP after <b>${seconds}</b> seconds.`;
        $('#btnSendOtp').attr('disabled', true);
        TaskAction.otpInterval = setInterval(() => {
            seconds--;
            timerElement.innerHTML = `You can request a new OTP after <b>${seconds}</b> seconds.`;
            if (seconds <= 0) {
                clearInterval(TaskAction.otpInterval);
                timerElement.textContent = "";
                $('#btnSendOtp').attr('disabled', false);
            }
        }, 1000);
    }
    static stopOtpTimer({ elementId }) {
        clearInterval(TaskAction.otpInterval);
        document.querySelector(elementId).textContent = "";
    }
    static add({ obj }) {
        Data.post({
            url: 'TaskAction/Add',
            data: obj,
            onSuccess: (response) => {
                Message.show(response);
                if (response.status == Message.Type.success) {
                    $('#Remarks').val(null);
                    let TaskAction = response.data.TaskAction;
                    Table.add({ id: '#tableTaskAction', data: TaskAction, action: "prepend" });
                    let Task = response.data.Task;
                    Table.updateById({ id: '#tableTask', objId: Task.Id, obj: Task });
                }
            }
        });
    }
    static delete({ id, index }) {
        Message.confirm({
            msg: "Do you want to delete??",
            confirmButtonText: "Delete",
            denyButtonText: "Don't Delete",
            data: id,
            onConfirm: (id) => {
                Data.delete({
                    url: `TaskAction/Delete?Id=${id}`,
                    onSuccess: (response) => {
                        Message.show(response);
                        if (response.status == Message.Type.success) {
                            Table.remove({ id: '#tableTaskAction', value: [index] });
                        }
                    }
                });
            }
        });
    }
}

window.tableTaskSlNo = (value, obj, index) => {
    return index + 1;
}
window.tableTaskInfo = (value, data, index) => {
    let labelStyle = "width:100px; font-weight: bold;";
    return `
        <div><label style="${labelStyle}">Date</label> <b>:</b> ${moment(data.Date).format('DD-MMM-YYYY')}</div>
        <div><label style="${labelStyle}">Ticket No.</label> <b>:</b> ${data.Code}</div>
        <div><label style="${labelStyle}">Party</label> <b>:</b> ${data.PartyDesc.match(/.{1,50}/g).join('<br>')}</div>
        <div><label style="${labelStyle}">Department</label> <b>:</b> ${data.PoNo}</div>
        <div><label style="${labelStyle}">Location</label> <b>:</b> ${data.PoDate}</div>
        <div><label style="${labelStyle}">Contact Person</label> <b>:</b> ${data.CreatedByName}</div>     
        <div><label style="${labelStyle}">Contact Person</label> <b>:</b> ${data.Description}</div>
        
    `;
}
window.tableTaskAction = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableTask = (value, obj, index) => {
    let actionBtn = [];
    if (obj.IsAddStatus) {
        actionBtn.push(`
            <li>
                <a href="#" class="dropdown-item text-success btn-new-entry" title="New Entry(Add Status)">
                    <span class="fa fa-plus"></span>&nbsp;&nbsp;Add Status
                </a>
            </li>
        `);
    }
    return `
        <div class="btn-group dropstart">
            <button type="button" class="btn btn-sm border-0" data-bs-toggle="dropdown">
                <i class="fa fa-ellipsis-v"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-lg-end mt-4">
                ${actionBtn.join('')}
            </ul>
        </div>
    `;
}
window.tableTaskEvent = {
    'click .btn-new-entry': (e, value, obj, row) => {
        obj.TaskId = obj.Id;
        obj.Ta = obj.Code;
        TaskAction.newEntry({ obj: obj });
    }
}

window.tableTaskActionSlNo = (value, obj, index) => {
    return index + 1; 
}
window.tableTaskActionStatus = (value, obj, index) => {
    return `<div class="${obj.StatusCss}">${obj.StatusDesc}</div>`;
}
window.tableTaskStatus = (value, data, index) => {
    return `<div class="${data.StatusCss}">${data.StatusDesc}</div>`;
}
window.tableTaskActionLogByAndAt = (value, obj, index) => {
    return `<div>${obj.CreatedByName}</div><div>${moment(obj.CreatedAt).format("DD-MMM-YYYY HH:mm")}</div>`;
}
window.tableTaskAction = (value, obj, index) => {
    return `<button type="button" class="btn btn-sm btn-danger rounded-5 btn-delete"><span class="fa fa-trash"></span></button>`;
}
window.tableTaskActionEvents = {
    'click .btn-delete': (e, value, obj, index) => {
        TaskAction.delete({ id: obj.Id, index: index });
    }
}
