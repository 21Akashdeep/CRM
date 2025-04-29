const App = {
    Setting: sessionStorage.getItem('Setting') ? JSON.parse(sessionStorage.getItem('Setting')) : null,
    User: sessionStorage.getItem('User') ? JSON.parse(sessionStorage.getItem('User')) : null,
    AppMenu: sessionStorage.getItem('AppMenu') ? sessionStorage.getItem('AppMenu') : '',
    Company: sessionStorage.getItem('Company') ? JSON.parse(sessionStorage.getItem('Company')) : null,
    Info: { Code: "CRM", Name: "CRM PORTAL", Desc: "Customer Relationship Managment", SubDesc: "Login" }
}

const Util = {
    init() {
        $('.view-password').on('click', function () {
            let id = $(this).closest('.input-group').find('input').attr('id');
            if ($(`#${id}`).attr('type') == 'password') {
                $(`#${id}`).attr('type', 'text');
                $(this).find('i').removeClass('fa-eye');
                $(this).find('i').addClass('fa-eye-slash');
            }
            else if ($(`#${id}`).attr('type') == 'text') {
                $(`#${id}`).attr('type', 'password');
                $(this).find('i').addClass('fa-eye');
                $(this).find('i').removeClass('fa-eye-slash');
            }
        });
        $('input[data-num="true"]').on('input', (event) => {
            const regex = /^[0-9.-]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            let value = filteredValue.replace(/^(.*?\..*?)\..*$/, '$1');
            let isNegative = value.startsWith('-');
            value = value.replaceAll('-', '');
            value = isNegative ? `-${value}` : value;

            //Validate Decimal Place
            if (event.currentTarget.attributes['data-decimal-place'] != undefined && value.includes('.')) {
                let num = value.split('.')[0].length == 0 ? '0' : value.split('.')[0];
                let decimal = value.split('.')[1];
                let decimalPlace = event.currentTarget.attributes['data-decimal-place'].value;
                decimalPlace = isNaN(decimalPlace) == false ? parseInt(decimalPlace) : 0;
                decimal = decimal.length > decimalPlace ? `.${decimal.substring(0, decimalPlace)}` : decimalPlace > 0 ? `.${decimal}` : '';
                value = `${num}${decimal}`;
            }
            //Validate Max Value
            if (event.currentTarget.attributes['data-max'] != undefined) {
                let max = event.currentTarget.attributes['data-max'].value;
                max = isNaN(max) == false ? parseInt(max) : 0;
                value = parseFloat(value) > max ? max : value;
            }
            event.currentTarget.value = value;
        });
        $('input[data-num="true"]').on('focusout', (event) => {
            let value = event.currentTarget.value;
            //Validate Min Value
            if (event.currentTarget.attributes['data-min'] != undefined) {
                let min = event.currentTarget.attributes['data-min'].value;
                min = isNaN(min) == false ? parseInt(min) : 0;
                value = parseFloat(value) < min ? min : value;
            }
            event.currentTarget.value = value;
        });
        $('input[data-alpha-name="true"]').on('input', (event) => {
            const regex = /^[A-Za-z ]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            event.currentTarget.value = filteredValue;
        });
        $('input[data-alpha="true"]').on('input', (event) => {
            const regex = /^[A-Za-z]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            event.currentTarget.value = filteredValue;
        });
        $('input[data-alpha-lg="true"]').on('input', (event) => {
            const regex = /^[A-Za-z ]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            event.currentTarget.value = filteredValue.toUpperCase();
        });
        $('input[data-alpha-sm="true"]').on('input', (event) => {
            const regex = /^[a-z]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            event.currentTarget.value = filteredValue.toLowerCase();
        });
        $('input[data-alpha-num="true"]').on('input', (event) => {
            const regex = /^[A-Za-z0-9]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            event.currentTarget.value = filteredValue;
        });
        $('input[data-alpha-lg-num="true"]').on('input', (event) => {
            const regex = /^[A-Za-z0-9]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            event.currentTarget.value = filteredValue.toUpperCase();
        });
        $('input[data-alpha-sm-num="true"]').on('input', (event) => {
            const regex = /^[A-Za-z0-9]*$/;
            const filteredValue = Array.from(event.currentTarget.value).filter(char => regex.test(char)).join('');
            event.currentTarget.value = filteredValue.toLowerCase();
        });
    }
};

const Url = {
    App: window.location.origin,
    Api: "http://localhost:15282/api/",
    /*Api: "https://pcatscrmapi.dharunam.in/api/",*/
    get() {
        const params = new URLSearchParams(window.location.search);
        const obj = {};
        for (const [key, value] of params.entries()) {
            obj[key] = value;
        }
        return obj;
    },
    query(name) {
        const params = new URLSearchParams(window.location.search);
        return params.has(name) ? decodeURIComponent(params.get(name)) : null;
    },
    getEndPoint() {
        return window.location.pathname.replace('/');
    }
};

const Cookie = {
    get(name) {
        const match = document.cookie.match(`(^| )${name}=([^;]+)`);
        return match ? match[2] : null;
    },
    set({ name, value, expiryDays = 100 }) {
        const expires = new Date(Date.now() + expiryDays * 86400000).toUTCString();        
        document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires}; path=/`;
    },
    remove(name) {
        document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/`;
    }
};

const Message = {
    Type: {
        success: 200,
        info: 202,
        warning: 204,
        error: 500,
        redirect: 302,
        notfound: 404,
        unauthorized: 401
    },
    timer: 5000,
    show({ status = Message.Type.success, statusText = "" }) {
        let heading = '';
        let icon = '';

        switch (status) {
            case Message.Type.success:
                heading = 'Success';
                icon = 'success';
                break;
            case Message.Type.warning:
                heading = 'Warning';
                icon = 'warning';
                break;
            case Message.Type.info:
                heading = 'Info';
                icon = 'info';
                break;
            case Message.Type.unauthorized:
                heading = 'Unauthorized';
                icon = 'error';
                break;
            case Message.Type.redirect:
                heading = 'Error';
                icon = 'error';
                break;
            default:
                heading = 'Error';
                icon = 'error';
                break;
        }

        $.toast({
            heading: heading,
            text: statusText,
            icon: icon,
            loader: true,
            loaderBg: '#5a08fc',
            bgColor: 'rgba(0,0,0,0.6)',
            textColor: 'white',
            showHideTransition: 'plain',
            position: 'top-right',
            stack: 1,
            hideAfter: Message.timer
        });

        if (status === Message.Type.redirect) {
            setTimeout(() => {
                location.replace(`${Url.App}${redirectPage}`);
            }, Message.timer);
        }
    },
    success({ statusText = "" }) {
        $.toast({
            heading: 'Success',
            text: statusText,
            icon: 'success',
            loader: true,
            loaderBg: '#5a08fc',
            bgColor: 'rgba(0,0,0,0.6)',
            textColor: 'white',
            showHideTransition: 'plain',
            position: 'top-right',
            stack: 1,
            hideAfter: Message.timer
        });
    },
    warn({ statusText = "" }) {
        $.toast({
            heading: 'Warning',
            text: statusText,
            icon: 'warning',
            loader: true,
            loaderBg: '#5a08fc',
            bgColor: 'rgba(0,0,0,0.6)',
            textColor: 'white',
            showHideTransition: 'plain',
            position: 'top-right',
            stack: 1,
            hideAfter: Message.timer
        });
    },
    error({ statusText = "" }) {
        $.toast({
            heading: 'Error',
            text: statusText,
            icon: 'error',
            loader: true,
            loaderBg: '#5a08fc',
            bgColor: 'rgba(0,0,0,0.6)',
            textColor: 'white',
            showHideTransition: 'plain',
            position: 'top-right',
            stack: 1,
            hideAfter: Message.timer
        });
    },
    alert({ status, statusText = "", redirectPage = "" }) {
        let swalHtml = "<div class='text-white'>";

        switch (status) {
            case Message.Type.success:
                swalHtml += `<span class='text-success'><i class='fa fa-check fa-4x'></i></span><h3>Success</h3>`;
                break;
            case Message.Type.warning:
                swalHtml += `<span class='text-warning'><i class='fa fa-exclamation-triangle fa-4x'></i></span><h3>Warning</h3>`;
                break;
            case Message.Type.error:
            case Message.Type.unauthorized:
                swalHtml += `<span class='text-danger'><i class='fa fa-remove fa-4x'></i></span><h3>${status === Message.Type.unauthorized ? "Unauthorized" : "Error"}</h3>`;
                break;
            case Message.Type.info:
                swalHtml += `<span class='text-info'><i class='fa fa-info-circle fa-4x'></i></span><h3>Info</h3>`;
                break;
            case Message.Type.redirect:
                swalHtml += `<span class='text-danger'><i class='fa fa-remove fa-4x'></i></span><h3>Error</h3>`;
                Swal.fire({
                    html: `${swalHtml}<div class='swal2-text-msg'><span>${statusText}</span></div>`,
                    background: 'rgba(0,0,0,0.5)',
                    timer: Message.timer
                });
                setTimeout(() => location.replace(`${Url.App}${redirectPage}`), Message.timer);
                return;
            default:
                try {
                    const errorDetails = JSON.parse(statusText);
                    swalHtml += `<span>Error: ${errorDetails.message}<br>File: ${errorDetails.file}<br>Line No.: ${errorDetails.line}</span>`;
                } catch (ex) {
                    swalHtml += `<span>${statusText}</span>`;
                }
                swalHtml += `<h3>Error</h3>`;
                break;
        }
        swalHtml += `<div class='swal2-text-msg'><span>${statusText}</span></div></div>`;        
        Swal.fire({
            html: swalHtml,
            background: 'rgba(0,0,0,0.5)',
            timer: 100000,
            didOpen: () => {
                document.querySelector('button.swal2-confirm').focus();                
            }
        });        
    },
    confirm({ msg = '', confirmButtonText = 'Confirm', denyButtonText = "Don't Confirm", data = null, onConfirm = () => { }, onDenied = () => { } }) {
        Swal.fire({
            html: `<div style='color:white'><h3>Are you sure??</h3><span>${msg}</span></div>`,
            background: 'rgba(0,0,0,0.5)',
            showDenyButton: true,
            confirmButtonText,
            denyButtonText,
        }).then((result) => {
            if (result.isConfirmed) onConfirm(data);
            if (result.isDenied) onDenied(data);
        });
    },
    prompt({ msg = '', confirmButtonText = 'Confirm', denyButtonText = "Don't Confirm", data = null, onConfirm = () => { }, onDenied = () => { } }) {
        Swal.fire({
            html: `<div style='color:white'>
                    <h3>Are you sure??</h3>
                    <span>${msg}</span><br><br>
                    <label>Remarks</label>
                    <input type='text' id="Prompt_Remarks" class='form-control form-control-sm'/>
                   </div>`,
            background: 'rgba(0,0,0,0.5)',
            showDenyButton: true,
            confirmButtonText,
            denyButtonText,
        }).then((result) => {
            if (result.isConfirmed) onConfirm(data, $('#Prompt_Remarks').val());
            if (result.isDenied) onDenied(data);
        });
    }
};

const PageLoader = {
    on() {
        $('#modalRequestProcess').modal('show');
    },
    off() {
        $('#modalRequestProcess').modal('hide');
    }
};

const Data = {
    get({ url = null, loader = true, async = true, isApi = true, onSuccess = () => { } }) {           
        let ApiUrl = isApi ? Url.Api + url : Url.App + url;
        let AuthToken = App.User ? `Bearer ${App.User.AuthToken}` : '';
        $.ajax({
            url: ApiUrl,
            type: 'Get',
            async: async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            cache: false,
            headers: {
                Authorization: AuthToken,
            },
            beforeSend: () => {
                if (loader) {
                    PageLoader.on();
                }
            },
            success: (response) => {
                if (response.status == Message.Type.redirect || response.status == Message.Type.unauthorized) {
                    Message.show(response);
                    setTimeout(() => {
                        location.replace(Url.App + response.redirectPage);
                    }, 3000);
                    return;
                }
                try {
                    onSuccess(response);
                } catch (ex) {
                    console.error(ex);
                }
            },
            error: (response) => {
                if (response.status == Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App);
                    }, 3000);
                    return;
                }
                else {
                    console.error(JSON.stringify(response));
                    Message.show(response);
                }
            },
            complete: () => {
                if (loader) {
                    PageLoader.off();
                }
            }
        });
    },
    post({ url = null, data = null, loader = true, loaderName = '', async = true, isApi = true, onSuccess = () => { } }) {
        let ApiUrl = isApi ? Url.Api + url : Url.App + url;
        let AuthToken = App.User ? `Bearer ${App.User.AuthToken}` : '';        
        $.ajax({
            url: ApiUrl,
            type: 'POST',
            async: async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            headers: {
                Authorization: AuthToken,
            },
            data: JSON.stringify(data),
            beforeSend: () => {
                if (loader) {
                    if (loaderName != '') {
                        $(loaderName).show();
                    }
                    else {
                        PageLoader.on();
                    }
                }
            },
            success: (response) => {
                if (response.status == Message.Type.redirect || response.status == Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App + response.redirectPage);
                    }, 3000);
                    return;
                }
                try {
                    onSuccess(response);
                } catch (ex) {
                    console.error(ex);
                }
            },
            error: (response) => {
                if (response.status == Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App);
                    }, 3000);
                    return;
                }
                else {
                    console.error(JSON.stringify(response));
                    Message.show(response);
                }
            },
            complete: () => {
                if (loader) {
                    if (loaderName != '') {
                        $(loaderName).hide();
                    }
                    else {
                        PageLoader.off();
                    }
                }
            }
        });
    },
    update({ url= null, data= null, loader= true, async= true, isApi= true, onSuccess= () => { } }) {
        let ApiUrl = isApi ? Url.Api + url : Url.App + url;
        let AuthToken = App.User ? `Bearer ${App.User.AuthToken}` : '';  
        $.ajax({
            url: ApiUrl,
            type: 'Patch',
            async: async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            headers: {
                Authorization: AuthToken,
            },
            data: JSON.stringify(data),
            beforeSend: () => {
                if (loader) {
                    PageLoader.on();
                }
            },
            success: (response) => {
                if (response.status == Message.Type.redirect || response.status == Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App + response.redirectPage);
                    }, 3000);
                    return;
                }
                try {
                    onSuccess(response);
                } catch (ex) {
                    console.error(ex);
                }
            },
            error: (response) => {
                if (response.status == Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App);
                    }, 3000);
                    return;
                }
                else {
                    console.error(JSON.stringify(response));
                    Message.show(response);
                }
            },
            complete: () => {
                if (loader) {
                    PageLoader.off();
                }
            }
        });
    },
    delete({ url= null, loader= true, async= true, isApi= true, onSuccess= () => { } }) {        
        let ApiUrl = isApi ? Url.Api + url : Url.App + url;
        let AuthToken = App.User ? `Bearer ${App.User.AuthToken}` : '';
        $.ajax({
            url: ApiUrl,
            type: 'Delete',
            async: async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            cache: false,
            headers: {
                Authorization: AuthToken,
            },
            beforeSend: () => {
                if (loader) {
                    PageLoader.on();
                }
            },
            success: (response) => {                
                try {
                    if (response.status == Message.Type.redirect || response.status == Message.Type.unauthorized) {
                        Message.alert(response);
                        setTimeout(() => {
                            location.replace(Url.App + response.redirectPage);
                        }, 3000);
                        return;
                    }
                    onSuccess(response);
                } catch (ex) {
                    console.error(ex);
                }
            },
            error: (response) => {
                if (response.status == Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App);
                    }, 3000);
                    return;
                }
                else {
                    console.error(JSON.stringify(response));
                    Message.show(response);
                }
            },
            complete: () => {
                if (loader) {
                    PageLoader.off();
                }
            }
        });
    },
    serializeToObject({ formId = '' } = {}) {
        if (!formId) return {}; // Return an empty object if formId is not provided or is empty
        const object = {};
        const array = $(formId).serializeArray();
        array.forEach(({ name, value }) => {
            if (value && value !== 'selectPick-Search-Clear') {
                object[name] = value || null; // Set value or null if empty
            }
        });
        return object;
    },
    objectToForm({ obj = {}, formId = '' }) {
        if (Array.isArray(obj)) {
            console.warn("Input data is not an object.");
            return;
        }
        if (!Object.keys(obj).length) {
            console.warn("Json object is empty.");
            return;
        }
        if (!formId) {
            console.warn("Form is not found.");
            return;
        }

        Object.keys(obj).forEach(key => {
            const val = obj[key] == null ? null : String(obj[key]);
            $(`${formId} [name=${key}]`).val(val);
        });
        //This usefull when call this method from modal function
        SelectPick.refresh({ selector: `${formId} select.select-pick` });
        $(`${formId} input.es-input`).editableSelect('destroy');
        $(`${formId} select.es-input`).editableSelect({ effects: 'slide' });
    },
    groupAndSum({ data, groupBy, sumBy = [] }) {
        const result = data.reduce((acc, current) => {
            // Create a group key based on the groupByFields
            const groupKey = groupBy.map(field => current[field]).join('-');

            // If the group doesn't exist in the result, initialize it
            if (!acc[groupKey]) {
                acc[groupKey] = groupBy.reduce((obj, field) => {
                    obj[field] = current[field]; // Add grouping fields
                    return obj;
                }, {});

                // Initialize the sum fields to 0
                sumBy.forEach(field => {
                    acc[groupKey][field] = 0;
                });
            }

            // Sum the fields
            sumBy.forEach(field => {
                acc[groupKey][field] += current[field];
            });

            return acc;
        }, {});

        // Return the result as an array
        return Object.values(result);
    },
    unique({ data = [], field = '' }) {
        try {
            // Create a Set from the values of the specified field to ensure uniqueness
            const uniqueValues = [...new Set(data.map(item => item[field]))];

            // Map the unique values into the required format
            return uniqueValues.map(value => ({ [field]: value }));
        } catch (ex) {
            console.error('Data & Field value is invalid.');
            return [];
        }
    },
    menuHtmlString(AppMenu) {
        var Menu = [];
        if (AppMenu.length > 0) {
            Menu.push('<ul class="menu">');
            AppMenu.forEach(function (gp, rowIndex) {
                let MenuId = gp.ApiGroupDesc.replaceAll(' ', "");
                Menu.push('<li class="root">');
                Menu.push(`
                    <a class="menu-item" href="#" data-target="#${MenuId}">
                        <i class="${gp.ApiGroupIcon}"></i>
                        <span>${gp.ApiGroupDesc}</span>
                        <i class="fa fa-angle-right"></i>
                    </a>`);
                Menu.push(`<ul id="${MenuId}" class="child">`);
                gp.Menu.forEach(function (me, rowIndex) {
                    Menu.push(`
                        <li>
                            <a class="menu-item" href="/${me.ApiName}">
                                <i class="${me.ApiIcon}"></i>
                                <span>${me.ApiDesc}</span>
                            </a>
                        </li>`);
                });
                //Sub Group
                gp.SubGroup.forEach(function (sgp, sgpRowIndex) {
                    let SubMenuId = `${MenuId}_${sgp.ApiGroupDesc.replaceAll(' ', '')}`;
                    Menu.push('<li class="root">');
                    Menu.push(`
                        <a class="menu-item" href="#" data-target="#${SubMenuId}">
                            <i class="${sgp.ApiGroupIcon}"></i>
                            <span>${sgp.ApiGroupDesc}</span>
                            <i class="fa fa-angle-right"></i>
                        </a>`);
                    Menu.push(`<ul id="${SubMenuId}" class="child">`);
                    sgp.Menu.forEach(function (sfm, sfmRowIndex) {
                        Menu.push(`
                            <li>
                                <a class="menu-item" href="~/${sfm.ApiName}">
                                    <i class="${sfm.ApiIcon}"></i>
                                    <span>${sfm.ApiDesc}</span>
                                </a>
                            </li>`);
                    });
                    Menu.push('</ul>');
                    Menu.push('</li>');
                });

                Menu.push('</ul>');
                Menu.push('</li>');
            });
            Menu.push('</ul>');
        }
        return Menu.join('');
    },
    isEmptyObj({ obj = {} }) {
        return Object.keys(obj).length === 0;
    },
    isJson({ value = null }) {
        try {
            const json = JSON.parse(value);
            return typeof json === 'object' && json !== null && Object.keys(json).length > 0;
        } catch {
            return false;
        }
    },
}

const _File = {
    attach({ fileId = "", allowedExtension = ['png', 'jpg', 'jpeg', 'pdf'], iframeId = "", readerOnLoad = () => { } }) {
        var objFile = $(fileId)[0];
        if (objFile.files.length > 0) {
            let FileName = objFile.files[0].name;
            let FileExtention = FileName.split('.')[FileName.split('.').length - 1];

            if (!allowedExtension.includes(FileExtention)) {
                _File.setIframe({ IframeId: iframeId, src: "/image/upload.png" });
                $(fileId).val(null);
                Message.error({ statusText: `Only attach ${allowedExtension.join(', ')}.` });
                return;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                const base64 = e.target.result.split(',')[1];
                const mimeType = e.target.result.split(',')[0].match(/:(.*?);/)[1];

                // Convert base64 to binary
                const byteCharacters = atob(base64);
                const byteNumbers = Array.from(byteCharacters, char => char.charCodeAt(0));
                const byteArray = new Uint8Array(byteNumbers);
                const blob = new Blob([byteArray], { type: mimeType });
                const blobUrl = URL.createObjectURL(blob);

                _File.setIframe({ iframeId: iframeId, mimeType: mimeType, src: blobUrl });
                $(iframeId).hide().fadeIn(650);
                let doc = {
                    Base64: base64,
                    MimeType: mimeType,
                    BlobUrl: blobUrl,
                    Blob: blob
                };
                readerOnLoad(doc);
            }
            reader.readAsDataURL(objFile.files[0]);
        }
        else {
            Message.error({ statusText: 'File did not attached.' });
        }
    },
    setIframe({ iframeId = "", mimeType = "", src = "" }) {
        if (Field.isNullOrEmpty(iframeId)) {
            console.warn("IFrame id is empty...");
            return;
        }
        if (Field.isNullOrEmpty(src)) {
            console.warn("Source is empty...");
            return;
        }
        if (mimeType == "application/pdf") {
            $(iframeId).removeAttr('srcdoc');
            $(iframeId).attr('src', src);
        }
        else {
            let htmlString = `
                <html>
                    <body style="text-align: center;">                        
                        <img class="border" src="${src}" style="max-width: 100%; height: auto;"/>
                    </body>
                </html>`;
            $(iframeId).attr('src', src);
            $(iframeId).attr('srcdoc', htmlString);
        }
    },
    async info(inputFile) {
        const file = inputFile.files[0];
        if (!file) return null;

        const base64 = await _File.read(file);
        return {
            Base64: base64.split(',')[1],
            MimeType: file.type
        };
    },
    read(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    },
    base64ToBytes(base64) {
        var binary_string = window.atob(base64);
        var len = binary_string.length;
        var bytes = new Uint8Array(len);
        for (var i = 0; i < len; i++) {
            bytes[i] = binary_string.charCodeAt(i);
        }
        return bytes.buffer;
    },
    open({ base64, mimeType, fileName }) {
        //Convert Byte Array to BLOB.
        var binary_string = window.atob(base64);
        var len = binary_string.length;
        var bytes = new Uint8Array(len);
        for (var i = 0; i < len; i++) {
            bytes[i] = binary_string.charCodeAt(i);
        }
        bytes = bytes.buffer;
        var blob = new Blob([bytes], { type: mimeType });

        //Create object URL and open in new tab
        const fileURL = URL.createObjectURL(blob);
        if (mimeType === 'application/pdf' || mimeType.startsWith('image/')) {
            const newTab = window.open('', '_blank');
            newTab.document.write(`
            <html>
                <head>
                    <title>${fileName}</title>                                    
                </head>
                <body style="margin:0">
                    <iframe src="${fileURL}" width="100%" height="100%" style="border:none;"></iframe>
                </body>
            </html>
        `);
        }
        else {
            const a = document.createElement('a');
            a.href = fileURL;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        }
    },
    blobUrl({ base64 = "", mimeType = "" }) {
        // Convert base64 to binary
        const byteCharacters = atob(base64);
        const byteNumbers = Array.from(byteCharacters, char => char.charCodeAt(0));
        const byteArray = new Uint8Array(byteNumbers);
        //Convert Binary to blob
        const blob = new Blob([byteArray], { type: mimeType });
        //Convert blob to Url
        const blobUrl = URL.createObjectURL(blob);
        return blobUrl;
    },
    parseUri(uri) {
        const regex = /^data:(.*?);base64,(.+)$/;
        const match = uri.match(regex);
        if (!match) {
            return null;
        }
        return {
            MimeType: match[1],
            Base64: match[2]
        };
    }
}

const Field = {
    isMandatory(data = { class: '', reset: false, id: '' }) {
        var status = true;
        data.class = typeof (data.class) == 'undefined' ? '' : data.class;
        data.reset = typeof (data.reset) == 'undefined' ? false : data.reset;
        data.id = typeof (data.id) == 'undefined' ? '' : data.id;
        //=================Reset All field=====================//
        if (data.reset && data.id != '') {
            $(`${data.id} [data-required=true]`).each(function () {
                $(this).removeAttr('data-bs-toggle');
                $(this).removeAttr('data-bs-placement');
                $(this).removeAttr('data-bs-original-title');
                $(this).removeAttr('data-required');
            });
        }
        //===================Check filed is Empty==================//
        if (data.class != '') {
            $(data.class).each(function () {
                if ($(this).attr('type') == "checkbox") {
                    if (!$(this).is(':checked')) {
                        $(this).attr('data-bs-toggle', 'tooltip');
                        $(this).attr('data-bs-placement', 'top');
                        $(this).attr('data-bs-original-title', 'Opps, Required Field');
                        $(this).attr('data-required', true);
                        $(this).tooltip();
                        status = false;
                    }
                }
                else {
                    if ($(this).attr('id') != undefined) {
                        if ($(this).val() == '' || $(this).val() == null) {
                            let element = this;
                            if ($(this).filter('.select-pick').length > 0) {
                                $(this).attr('data-required', true);
                                element = $(`[data-id="${this.id}"]`).find('.select-pick-toggle-btn');
                            }
                            $(element).attr('data-bs-toggle', 'tooltip');
                            $(element).attr('data-bs-placement', 'top');
                            $(element).attr('data-bs-original-title', 'Opps, Required Field');
                            $(element).attr('data-required', true);
                            $(element).tooltip();
                            status = false;
                        }
                    }
                }
            });
            //===============Reset field when it's take some input================//
            $('textarea[data-required=true], input[data-required=true]').on('input', function () {
                let element = this;
                $(element).removeAttr('data-bs-toggle');
                $(element).removeAttr('data-bs-placement');
                $(element).removeAttr('data-bs-original-title');
                $(element).removeAttr('data-required');
            });
            $('select[data-required=true]').on('change', function () {
                let element = this;
                if ($(this).filter('.select-pick').length > 0) {
                    $(this).removeAttr('data-required');
                    element = $(`[data-id="${this.id}"]`).find('.select-pick-toggle-btn');
                }
                $(element).removeAttr('data-bs-toggle');
                $(element).removeAttr('data-bs-placement');
                $(element).removeAttr('data-bs-original-title');
                $(element).removeAttr('data-required');
            });
        }
        if (!status) {
            Message.show({ status: Message.Type.error, statusText: 'Please fill all required field.<br>Whose border color is red.' });
        }
        return status;
    },
    isNullOrEmpty: (string) => !string?.length,
    triggerOnInput(id) {
        const event = new Event('input', { bubbles: true });
        const element = document.querySelectorAll(id);
        element.forEach(el => { el.dispatchEvent(event) });
    },
    triggerOnChange(id) {
        const event = new Event('change', { bubbles: true });
        const element = document.querySelectorAll(id);
        element.forEach(el => { el.dispatchEvent(event) });
    },
};

const Modal = {
    open(pram = { id: '', title: '', action: '', obj: {}, callBack: () => { } }) {
        pram.id = typeof (pram.id) == 'undefined' ? '' : pram.id;
        pram.title = typeof (pram.title) == 'undefined' ? '' : pram.title;
        pram.action = typeof (pram.action) == 'undefined' ? '' : pram.action;
        pram.obj = typeof (pram.obj) == 'undefined' ? {} : pram.obj;
        //=======Check Modal Id====================//
        if (pram.id == '') {
            console.warn('Modal id did not find.');
        }
        if ($(pram.id).hasClass("show")) {
            $(pram.id).trigger('focus');
            return;
        }
        //=======Reset Modal Elements====================//
        $(pram.id + ' .modal-title').text(pram.title);
        $(pram.id + ' input, ' + pram.id + ' textarea,' + pram.id + ' select,' + pram.id + ' button').prop('disabled', false);
        $(pram.id + ' input[type=checkbox]').prop('checked', false);
        $(pram.id + ' input.es-input').editableSelect('destroy');
        $(pram.id + ' input.es-input').editableSelect({ effects: 'slide' });
        $(pram.id + ' input,' + pram.id + ' textarea,' + pram.id + ' select').val('');
        SelectPick.refresh({ selector: `${pram.id} select.select-pick` });
        $(pram.id + ' .modal-footer button').show();
        Table.empty({ selector: pram.id + ' .table-default' });

        Field.isMandatory({ reset: true, id: pram.id });
        $(pram.id + ' .date-picker').prevAll('input[type="text"]').each((index, obj) => {
            $('#' + obj.id).val(moment().format('DD-MMM-YYYY'));
        });
        $(pram.id + ' .date-time-picker').prevAll('input[type="text"]').each((index, obj) => {
            $('#' + obj.id).val(moment().format('DD-MMM-YYYY HH:mm'));
        });
        $(pram.id + ' .date-range-picker').prevAll('input[type="text"]').each((index, obj) => {
            $('#' + obj.id).val(moment().format('DD-MMM-YYYY') + ' | ' + moment().format('DD-MMM-YYYY'));
        });
        $(pram.id + ' .month-picker').prevAll('input[type="text"]').each((index, obj) => {
            $('#' + obj.id).val(moment().format('MMM-YYYY'));
        });
        $(pram.id + ' .time-picker').prevAll('input[type="text"]').each((index, obj) => {
            $('#' + obj.id).val(moment().format('HH:mm'));
        });

        _File.setIframe({ iframeId: `#${$(`${pram.id} iframe`).attr('id')}`, src: "/image/upload.png" });

        //=======Callback function===========//
        if (typeof (pram.callBack) != 'undefined') {
            pram.callBack();
            $(pram.id + ' input.es-input').editableSelect('destroy');
            $(pram.id + ' input.es-input').editableSelect({ effects: 'slide' });
            SelectPick.refresh({ selector: `${pram.id} select.select-pick` });
        }

        //=======Assign Values from Obj===========//
        if (Object.keys(pram.obj).length > 0) {
            let formId = '#' + $(pram.id + ' form').attr('id');
            Data.objectToForm({ obj: pram.obj, formId: formId });
            //======Refresh Select Pick==================//
            Dropdown.refresh({ selector: `${pram.id} select.select-pick` });

            //======Refresh Editable Select Picker==================//
            $(pram.id + ' input.es-input').editableSelect('destroy');
            $(pram.id + ' input.es-input').editableSelect({ effects: 'slide' });
        }
        //for Select 2 open properly in modal
        $.fn.modal.Constructor.prototype.enforceFocus = () => { };
        //======Modal Footer Button===================//
        if (pram.action == 'Add' || pram.action == 'add') {
            $(pram.id + ' .modal-footer button').attr('title', 'Save');
            /*$(pram.id + ' .modal-footer button').html('<span class="fa fa-save"></span>');*/
            $(pram.id).modal('show');
        }
        else if (pram.action == 'Edit' || pram.action == 'edit') {
            $(pram.id + ' .modal-footer button').attr('title', 'Update');            
            $(pram.id).modal('show');
        }
        else if (pram.action == 'View' || pram.action == 'view') {
            $(pram.id + ' .modal-body input').prop('disabled', true);
            $(pram.id + ' .modal-body textarea').prop('disabled', true);
            $(pram.id + ' .modal-body select').prop('disabled', true);
            SelectPick.refresh({ selector: `${pram.id} select.select-pick` });
            $(pram.id + ' .modal-footer button').hide();
            $(pram.id).modal('show');
        }
        else {
            $(pram.id).modal('show');
        }
    },
    reset(pram = { id: '' }) {
        pram.id = typeof (pram.id) == 'undefined' ? '' : pram.id;
        //=======Check Modal Id====================//
        if (pram.id == '') {
            console.warn('Modal id did not find.');
        }
        //=======Reset Modal Element====================//                
        $(pram.id + ' input, ' + pram.id + ' textarea,' + pram.id + ' select,' + pram.id + ' button').prop('disabled', false);
        $(pram.id + ' input[type=checkbox]').prop('checked', false);
        $(pram.id + ' input.es-input').editableSelect('destroy');
        $(pram.id + ' input.es-input').editableSelect({ "effects": 'slide' });
        $(pram.id + ' input,' + pram.id + ' textarea,' + pram.id + ' select').val('');
        SelectPick.refresh({ selector: `${pram.id} select.select-pick` });
        Table.empty({ selector: pram.id + ' .table-default' });
        Field.isMandatory({ reset: true, id: pram.id });
        $('#' + $(pram.id + ' .date-picker').prev('input').attr('id')).val(moment().format('DD-MMM-YYYY'));
        $('#' + $(pram.id + ' .date-range-picker').prev('input').attr('id')).val(moment().format('DD-MMM-YYYY') + ' | ' + moment().format('DD-MMM-YYYY'));
        $('#' + $(pram.id + ' .month-picker').prev('input').attr('id')).val(moment().format('MMM-YYYY'));
        _File.setIframe({ IframeId: $(pram.id + ' .file-upload iframe').attr('id'), src: "/images/uploadlogo.png" });
    },
    close(pram = { id: '' }) {
        pram.id = typeof (pram.id) == 'undefined' ? '' : pram.id;
        //=======Check Modal Id====================//
        if (pram.id == '') {
            console.warn('Modal id did not find.');
        }
        //=======Reset Modal Element====================//                
        $(pram.id + ' input, ' + pram.id + ' textarea,' + pram.id + ' select,' + pram.id + ' button').prop('disabled', false);
        $(pram.id + ' input[type=checkbox]').prop('checked', false);
        $(pram.id + ' input.es-input').editableSelect('destroy');
        $(pram.id + ' input.es-input').editableSelect({ "effects": 'slide' });
        $(pram.id + ' input,' + pram.id + ' textarea,' + pram.id + ' select').val('');
        SelectPick.refresh({ selector: `${pram.id} select.select-pick` });
        Table.empty({ selector: pram.id + ' .table-default' });
        Field.isMandatory({ reset: true, id: pram.id });
        $('#' + $(pram.id + ' .date-picker').prev('input').attr('id')).val(moment().format('DD-MMM-YYYY'));
        $('#' + $(pram.id + ' .date-range-picker').prev('input').attr('id')).val(moment().format('DD-MMM-YYYY') + ' | ' + moment().format('DD-MMM-YYYY'));
        $('#' + $(pram.id + ' .month-picker').prev('input').attr('id')).val(moment().format('MMM-YYYY'));
        _File.setIframe({ IframeId: $(pram.id + ' .file-upload iframe').attr('id'), src: "/images/uploadlogo.png" });
        $(pram.id).modal('hide');
    },
    hideEvent() {
        document.querySelectorAll('.modal').forEach(modal => {
            modal.removeEventListener('hide.bs.modal', Modal.handleModalHide); // remove if already attached
            modal.addEventListener('hide.bs.modal', Modal.handleModalHide);
        });
    },
    handleModalHide() {
        if (document.activeElement) {
            document.activeElement.blur();
        }
    }
};

const Print = {
    page({ title = 'Print Report', link = [], style = [], content = [], border = false, isPrint = true, orientation = 'A4 portrait' }) {
        if (content.length == 0) {
            Message.error({ statusText: 'Content is empty.' });
            return;
        }
        style.push(`
            body{font-family: Calibri, sans-serif;}
            @media print{
                .not-print{display:none;}
            }
            @page {
                size:${orientation}; 
                margin-top:3mm;
                margin-left:3mm;
                margin-right:3mm;
                margin-bottom:10mm;                
                @bottom-right { 
                    content: "Page " counter(page) " of " counter(pages)                    
                };
            }            
            .page-container {
                display: flex;
                justify-content: center;
                align-items: center;
                margin-bottom:10px;
            }
            .page {
                ${orientation == 'A4 portrait' ? 'width:199mm; height: 276mm;' : orientation == 'A4 landscape' ? 'width:287mm; height: 200mm;' : ''}    
                padding: 10px; 
                page-break-after: always;
                border: ${border ? '1px solid black' : 'none'};                
                
            }
            .not-print{height: 100px!important; border:none!important; width:98.5%;}
            table{border-collapse: collapse;} 
            table td, th{padding-left:5px; padding-right:5px;}
            th{text-align:left;}
            sub-table td th{padding-left:0;}
            .border{border:1px solid black;}
            .border-left{border-left:1px solid black;}
            .border-right{border-right:1px solid black;}
            .border-top{border-top:1px solid black;}
            .border-bottom{border-bottom:1px solid black;}
            .border-none-left{border-left:none;}
            .border-none-right{border-right:none;}
            .border-none-top{border-top:none;}
            .border-none-bottom{border-bottom:none;}
            .text-left{text-align:left}
            .text-center{text-align:center;}
            .text-right{text-align:right}
            .text-underline{text-decoration:underline;}
            .v-align-top{vertical-align: top;}
            .v-align-middle{vertical-align: middle;}
            .v-align-bottom{vertical-align: bottom;}
            .pl-0{padding-left: 0;}
            .pr-0{padding-right: 0;}
            .mt-2{margin-top:10px;}
            .wrap-all{word-break: break-all;}
            .fw-bold{font-weight:bold;}
            .fw-normal{font-weight:normal;}            
            .text-nowrap{white-space: nowrap;}
            .brand-name{font-size:20px; font-weight:bold;}
            .brand-title{font-size:14px; font-weight:bold;}
            .btn-print{color:#212529; border-color: #212529; border-radius:5px; background-color:#fff; padding:3px 10px 3px 10px; font-size:14px;}
            .btn-print:hover{color:#fff; border-color: #212529; border-radius:5px; background-color:#212529}            
        `);
        var printWindow = window.open('', '_blank', `top = 0, left = 0, height = ${screen.height}, width = ${screen.width}`);
        printWindow.document.write(`
            <html>
                <head>
                    <title>${title}</title>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <link rel="stylesheet" href="${Url.App}/lib/font-awesome/css/all.css" rel="stylesheet" />
                    ${link.join('')}
                    <style>${style.join('')}</style>
                    <script type="text/javascript" asp-append-version="true" src="${Url.App}/lib/jquery/jquery.js"></script>
                    <script type="text/javascript" asp-append-version="true" src="${Url.App}/script/util.js"></script>
                </head>
                <body>
                    ${content.join('')}
                    <div class="page not-print text-center mt-2">                        
                        <button type="button" class="btn-print" onClick="window.print(); window.close();">
                            <span class="fa fa-print"></span>&nbsp;&nbsp;Print
                        </button>
                    </div>                 
                </body>
            </html>
        `);
        if (printWindow.document == null) {
            Message.error({ statusText: 'Opps, print pop up is block in your browser.<br>Please unblock it.' });
            return;
        }
        printWindow.document.close();
        printWindow.focus();
        if (isPrint) {
            setTimeout(function () {
                printWindow.print();
                printWindow.close();
            }, 1000);
        }
    },
    table({ title = 'Print Report', link = [], style = [], content = [], border = false, isPrint = true, orientation = 'A4 portrait' }) {
        if (content.length == 0) {
            Message.error({ statusText: "Print content is empty." });
            return;
        }
        style.push(`
            body{
                font-family: Calibri, sans-serif;
            }
            @media print{
                @page { size:${orientation}; margin:7mm; counter-increment:page;  @bottom-right { content: "Page " counter(page) " of " counter(pages)}; }
                table { page-break-after: auto; border-collapse: collapse; width:100%; }                
                thead { display: table-header-group; }
                tfoot { display: table-footer-group; }
                tr { page-break-inside: avoid; page-break-after: auto; }
                td { page-break-inside: avoid; page-break-after: auto; border: ${border ? `1px solid black;` : `none;`}}
                th { page-break-inside: avoid; page-break-after: auto; border: ${border ? `1px solid black;` : `none;`}}                                                
                .not-print-item{display:none;}
            }
            td.td-num { vertical-align:middle; text-align: right; white-space: nowrap; }
            .th-heading { font-size:14px;}
            .sub-table td th{ padding-left:0; }
            p{ margin:0; }
            .border{border:1px solid black;}
            .border-left{border-left:1px solid black;}
            .border-right{border-right:1px solid black;}
            .border-top{border-top:1px solid black;}
            .border-bottom{border-bottom:1px solid black;}
            .text-left{text-align:left}
            .text-center{text-align:center;}
            .text-right{text-align:right}
            .fw-bold{font-weight:bold}
            .v-align-top{vertical-align: top;}
            .v-align-middle{vertical-align: middle;}
            .v-align-bottom{vertical-align: bottom;}
            .p-0{padding: 0;}
            .pl-0{padding-left: 0 !important;}
            .pr-0{padding-right: 0 !important;}
            .wrap-all {word-break: break-all;}
            .text-nowrap {white-space:nowrap;}
            .text-wrap {overflow-wrap: break-word;}
            .grid-container-2 {display: grid; grid-template-columns: 50% 50%;}
            .grid-container-4 {display: grid; grid-template-columns: 25% 25% 25% 25%;}
            .grid-item {padding-left:5px; padding-right:5px;}
            .btn-group{position: relative; display: inline-flex; vertical-align: middle;}
            .company-name, .company-add, .report-desc{text-align:center;}
            .company-name {font-size:18px;}
            .company-add, .report-desc {font-size:14px;}
            .btn-print { color:#212529; border-color: #212529; border-radius:5px; background-color:#fff; padding:3px 10px 3px 10px; font-size:14px;}
            .btn-print:hover { color:#fff; border-color: #212529; border-radius:5px; background-color:#212529;}
            .not-print-item{ margin-top:20px;}
        `);

        var printWindow = window.open('', '_blank', `top = 0, left = 0, height = ${screen.height}, width = ${screen.width}`);
        printWindow.document.write(`
            <html>
                <head>
                    <title>${title}</title>
                    <link rel="stylesheet" href="${Url.App}/lib/font-awesome/css/all.css" rel="stylesheet" />
                    ${link.join('')}
                    <style>${style.join('')}</style>
                    <script type="text/javascript" asp-append-version="true" src="${Url.App}/lib/jquery/jquery.js"></script>
                    <script type="text/javascript" asp-append-version="true" src="${Url.App}/script/util.js"></script>                                        
                </head>
                <body>
                    ${content.join('')}                    
                    <div class="text-center not-print-item">
                        <button type="button" class="btn-print text-center" onClick="window.print(); window.close();">
                            <span class="fa fa-print"></span>&nbsp;&nbsp;Print
                        </button>
                    </div>                    
                </body>
            </html>
        `);
        if (printWindow.document == null) {
            Message.error({ statusText: 'Opps, print pop up is block in your browser.<br>Please unblock it.' });
            return;
        }
        printWindow.document.close();
        printWindow.focus();
        if (!isPrint) {
            setTimeout(function () {
                printWindow.print();
                printWindow.close();
            }, 1000);
        }
    },
    sticker(pram = { title: '', link: [], style: [], content: [], pageBorder: false, print: true, orientation: 'portrait', width: 4, height: 4, measurement: 'in' }) {
        pram = {
            title: typeof (pram.title) == 'undefined' ? 'Print Report' : pram.title,
            link: typeof (pram.link) == 'undefined' ? [] : pram.link,
            style: typeof (pram.style) == 'undefined' ? [] : pram.style,
            content: typeof (pram.content) == 'undefined' ? [] : pram.content,
            pageBorder: typeof (pram.pageBorder) == 'undefined' ? false : pram.pageBorder,
            print: typeof (pram.print) == 'undefined' ? true : pram.print,
            orientation: typeof (pram.orientation) == 'undefined' ? 'portrait' : pram.orientation,
            width: typeof (pram.width) == 'undefined' ? 4 : pram.width,
            height: typeof (pram.height) == 'undefined' ? 4 : pram.height,
            measurement: typeof (pram.measurement) == 'undefined' ? 'in' : pram.measurement,
        };
        pram.style.push('@media print{');
        pram.style.push('.print-page {page-break-after: always; overflow:hidden;}');
        if (pram.orientation == 'portrait') {
            pram.style.push('@page { size:' + pram.orientation + '; size:' + pram.width + pram.measurement + ' ' + (pram.height) + pram.measurement + '; margin:0.18rem 0.5rem 0.5rem 0.18rem}');
        }
        else {
            pram.style.push('@page { size:' + pram.orientation + '; size:' + pram.height + pram.measurement + ' ' + (pram.width) + pram.measurement + '; margin:0.18rem 0.5rem 0.5rem 0.18rem}');
        }

        pram.style.push('.btn-print{ display:none; }');
        pram.style.push('.not-print-div{ display:none; }');
        pram.style.push('}');

        pram.style.push('th{text-align:left;}');
        pram.style.push('.print-page {page-break-after: always; overflow:hidden;}');
        if (pram.orientation == 'portrait') {
            pram.style.push('.print-page {width:' + pram.width + pram.measurement + '; height: ' + (pram.height - 0.2) + pram.measurement + ';}');
        }
        else {
            /*pram.style.push('.print-page { transform:translate(-2.09in,5.65in) rotate(90deg); transform-origin: top right; width:' + (pram.height - 0.2) + pram.measurement + '; height: ' + pram.width + pram.measurement + '; }');*/
            pram.style.push('.print-page { transform:translate(-2.09in,5.65in) rotate(90deg); transform-origin: top right; width:' + (pram.width - 0.0) + pram.measurement + '; height: ' + pram.height + pram.measurement + '; }');
        }
        if (pram.pageBorder == true) {
            pram.style.push('.print-page {border: 1px solid red; }');
        }

        pram.style.push('.table-child td th{ padding-left:0; }');
        pram.style.push('.border{border:1px solid black;}')
        pram.style.push('.border-left{border-left:1px solid black;}');
        pram.style.push('.border-right{border-right:1px solid black;}');
        pram.style.push('.border-top{border-top:1px solid black;}');
        pram.style.push('.border-bottom{border-bottom:1px solid black;}');
        pram.style.push('.text-left{text-align:left}');
        pram.style.push('.text-center{text-align:center}');
        pram.style.push('.text-right{text-align:right}');
        pram.style.push('.v-align-top{vertical-align: top;}');
        pram.style.push('.v-align-middle{vertical-align: middle;}');
        pram.style.push('.v-align-bottom{vertical-align: bottom;}');
        pram.style.push('.pl-0{padding-left: 0;}');
        pram.style.push('.pr-0{padding-right: 0;}');
        pram.style.push('.text-break-all {word-break: break-all;}');
        pram.style.push('.text-nowrap {white-space: nowrap;}');
        pram.style.push('.btn-print { color:#212529; border-color: #212529; border-radius:5px; background-color:#fff; padding:3px 10px 3px 10px; font-size:14px;}');
        pram.style.push('.btn-print:hover { color:#fff; border-color: #212529; border-radius:5px; background-color:#212529}');

        if (pram.content.length > 0) {
            var printWindow = window.open('', '_blank', 'top=0, left=0, height=' + screen.height + ', width=' + screen.width);
            printWindow.document.write('<html>');
            printWindow.document.write('<head>');
            printWindow.document.write('<title>');
            printWindow.document.write(pram.title);
            printWindow.document.write('</title>');
            printWindow.document.write('<link rel="stylesheet" href="' + Url.App + '/lib/font-awesome/css/all.css" rel="stylesheet" />');
            printWindow.document.write(pram.link.join(''));
            printWindow.document.write('<style>');
            printWindow.document.write(pram.style.join(''));
            printWindow.document.write('</style>');
            printWindow.document.write('</head>');
            printWindow.document.write('<body>');
            printWindow.document.write(pram.content.join(''));
            printWindow.document.write('<div class="text-center print-page not-print-div">');
            printWindow.document.write('<br/>');
            printWindow.document.write('<button type="button" class="btn-print" onClick="window.print(); window.close();" style="margin-right:10px;">');
            printWindow.document.write('<span class="fa fa-print"></span>&nbsp;&nbsp;Print');
            printWindow.document.write('</button>');
            printWindow.document.write('</div>');
            printWindow.document.write('</body>');
            printWindow.document.write('</html>');
            if (printWindow.document == null) {
                Message.show({ status: Message.error, statusText: 'Opps, print pop up is block in your browser.<br>Please unblock it.' });
            } else {
                printWindow.document.close();
                printWindow.focus();
                if (pram.print) {
                    setTimeout(() => {
                        printWindow.print();
                        printWindow.close();
                    }, 1000);
                }
            }
        } else {
            Message.show({ status: Message.error, statusText: 'Opps, Print content is empty.' });
        }
    },
}
const Export = {
    HtmlTableToExcel({ tableId = [], fileName = 'Download' }) {
        fileName = `${fileName}.xls`;

        const tableHtml = tableId.map(id => {
            const table = $(id)[0];
            return table.outerHTML.replace(/ /g, '%20').replaceAll("<br>", ",&nbsp;").replaceAll("₹", "");
        });

        const downloadLink = document.createElement("a");
        document.body.appendChild(downloadLink);
        downloadLink.href = `data:application/vnd.ms-excel; charset=utf-8,${tableHtml.join('<br>')}`;
        downloadLink.download = fileName;
        downloadLink.click();
        document.body.removeChild(downloadLink);
    },
    Base64ToExcel({ base64 = '', fileName = 'Download' }) {
        fileName = `${fileName}.xlsx`;
        if (!base64) {
            console.warn('Base64 string is empty');
            return;
        }

        const binaryString = window.atob(base64);
        const bytes = new Uint8Array(binaryString.length);

        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        const blob = new Blob([bytes], { type: "application/octet-stream" });
        const isIE = !!document.documentMode;

        if (isIE) {
            // For Internet Explorer
            window.navigator.msSaveBlob(blob, fileName);
        } else {
            // For other browsers
            const link = document.createElement('a');
            const url = window.URL.createObjectURL(blob);

            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }
}

const OnlineApi = {
    pinCode(pram = { pinCode: '', postOfficeId: '', postOffice: '', districtId: '', stateId: '', loader: false, callBack: () => { } }) {
        pram = {
            pinCode: pram.pinCode == undefined ? '' : pram.pinCode,
            postOfficeId: pram.postOfficeId == undefined ? '#PostOffice' : pram.postOfficeId,
            postOffice: pram.postOffice == undefined ? null : pram.postOffice,
            districtId: pram.districtId == undefined ? '#DistrictId' : pram.districtId,
            stateId: pram.stateId == undefined ? '#SateId' : pram.stateId,
            loader: pram.loader == undefined ? true : pram.loader,
            callBack: pram.callBack == undefined ? () => { } : pram.callBack
        };
        let data = {
            status: Message.Type.warning,
            statusText: 'No records found',
            obj: {}
        };
        $(pram.postOfficeId).editableSelect('destroy');
        $(pram.districtId).val(null);
        Dropdown.set({ id: pram.stateId });
        if (pram.pinCode.length != 6) {
            pram.callBack(data);
            return;
        }
        $.ajax({
            url: 'https://api.postalpincode.in/pincode/' + pram.pinCode,
            type: 'Get',
            dataType: 'json',
            contenttype: 'application/json; charset=utf-8',
            beforeSend: () => {
                if (pram.loader)
                    PageLoader.on();
            },
            success: (response) => {
                response = response[0]
                data.status = response.Status == 'Success' ? Message.Type.success : Message.Type.error;
                data.statusText = response.Message;
                data.obj.PostOffice = response.PostOffice == null ? [] : response.PostOffice;
                data.obj.District = response.PostOffice == null ? null : response.PostOffice[0].District;
                data.obj.State = response.PostOffice == null ? null : response.PostOffice[0].State;
                Dropdown.bind({ id: pram.postOfficeId, data: data.obj.PostOffice, value: 'Name', text: 'Name', isEditable: true });
                $(pram.postOfficeId).val(pram.postOffice);
                $(pram.districtId).val(data.obj.District);
                Dropdown.set({ id: pram.stateId, text: [data.obj.State] });
                pram.callBack(data);
            },
            error: (response) => {
                Message.error({ statusText: `Pin Code server is not responding.<br>${response.statusText}` });
                pram.callBack(obj);
            },
            complete: () => {
                if (pram.loader)
                    PageLoader.off();
            }
        });
    },
    ifscCode(pram = { ifscCode: '', bankNameId: '', bankAddressId: '', loader: true, callback: () => { } }) {
        pram = {
            ifscCode: pram.ifscCode == undefined ? '' : pram.ifscCode,
            bankNameId: pram.bankNameId == undefined ? '#BankName' : pram.bankNameId,
            bankAddressId: pram.bankNameId == undefined ? '#BankAddressId' : pram.bankAddressId,
            loader: pram.loader == undefined ? true : pram.loader,
            callback: pram.callback == undefined ? () => { } : pram.callback
        };
        $(`${pram.bankNameId}, ${pram.bankAddressId}`).val(null);
        let data = {
            status: Message.Type.warning,
            statusText: "IFSC Code did not find",
            obj: null
        };
        if (pram.ifscCode.length != 11) {
            pram.callback(data);
            return;
        }

        $.ajax({
            url: 'https://ifsc.razorpay.com/' + pram.ifscCode,
            type: 'Get',
            dataType: 'json',
            contenttype: 'application/json; charset=utf-8',
            beforSend: () => {
                if (pram.loader) {
                    PageLoader.on();
                }
            },
            success: (response) => {
                data.status = Message.Type.success;
                data.statusText = "Record found";
                data.obj = response;
                //Set Value
                let Address = (`${response.BRANCH}, ${response.ADDRESS}, ${response.CITY}, ${response.DISTRICT}, ${response.STATE}`).replaceAll(",,", ",");
                $(pram.bankNameId).val(data.obj.BANK);
                $(pram.bankAddressId).val(Address.toCamelCase());
                //Call Call function
                pram.callback(data);
            },
            error: (response) => {
                data.statusText = `IFSC Code ${pram.ifscCode} ${response.statusText}`;
                pram.callback(data);
            },
            complete: () => {
                if (pram.loader) {
                    PageLoader.off();
                }
            }
        });
    },
}
const DateTime = {
    init() {
        DateTime.monthPicker();
        DateTime.datePicker();
        DateTime.dateTimePicker();
        DateTime.timePicker();
        DateTime.dateRangePicker();
        DateTime.dateRangeTimePicker();
    },
    pickerFunction() { },
    monthPicker() {
        var inputId = "";
        $('.month-picker').each((index, element) => {
            inputId = '#' + $(element).prev('input').attr('id');
            if ($(inputId).val() == '') {
                $(inputId).val(moment().format("MMM-YYYY"));
            }
        });

        $('.month-picker').on('click', (e) => {
            inputId = '#' + $(e.currentTarget).prev('input').attr('id');
        });

        $('.month-picker').daterangepicker({
            showDropdowns: true,
            autoApply: true,
            minDate: moment().add(-2, 'y'),
            maxDate: moment().add(1, 'y'),
            singleDatePicker: true,
            autoUpdateInput: false,
            locale: {
                format: 'MMM-YYYY',
                separator: "|",
                firstDay: 1
            },
            linkedCalendars: true
        },
            (fromDate) => {
                if (inputId !== "") {
                    $(inputId).val(moment(fromDate).format("MMM-YYYY"));
                }
                DateTime.pickerFunction();
            }).on('showCalendar.daterangepicker', (ev, picker) => {
                if (picker.element.offset().top + picker.container.outerHeight() > $(window).height()) {
                    picker.drops = 'up';
                } else {
                    picker.drops = 'down';
                }
            });
        $('.month-picker').on('apply.daterangepicker', (ev, picker) => {
            $(inputId).val(picker.startDate.format('MMM-YYYY'));
        });
    },
    datePicker({ minDate, maxDate } = {}) {
        minDate = minDate === undefined ? moment().add(-10, 'y') : minDate;
        maxDate = maxDate === undefined ? moment().add(10, 'y') : maxDate;
        var inputId = "";
        $('.date-picker').each((index, element) => {
            inputId = '#' + $(element).prev('input').attr('id');
            if ($(inputId).val() == '') {
                $(inputId).val(moment().format("DD-MMM-YYYY"));
            }
        });

        $('.date-picker').on('click', (e) => {
            inputId = '#' + $(e.currentTarget).prev('input').attr('id');
            DateTime.pickerFunction = () => { }
        });

        $('.date-picker').daterangepicker({
            showDropdowns: true,
            autoApply: true,
            minDate: minDate,
            maxDate: maxDate,
            singleDatePicker: true,
            autoUpdateInput: false,
            locale: {
                format: 'DD-MMM-YYYY',
                separator: "|",
                firstDay: 1
            },
            linkedCalendars: true
        },
            (fromDate) => {
                if (inputId !== "") {
                    $(inputId).val(moment(fromDate).format("DD-MMM-YYYY"));
                }
                DateTime.pickerFunction();
            }).on('showCalendar.daterangepicker', (ev, picker) => {
                if (picker.element.offset().top + picker.container.outerHeight() > $(window).height()) {
                    picker.drops = 'up';
                } else {
                    picker.drops = 'down';
                }
            });
        $('.date-picker').on('apply.daterangepicker', (ev, picker) => {
            $(inputId).val(picker.startDate.format('DD-MMM-YYYY'));
        });
    },
    timePicker() {
        var inputId = "";
        $('.time-picker').each((index, element) => {
            inputId = '#' + $(element).prev('input').attr('id');
            if ($(inputId).val() == '') {
                $(inputId).val(moment().format("HH:mm")).trigger('change');
            }
        });
        $('.time-picker').on('click', (e) => {
            $inputId = '#' + $(e.currentTarget).prev('input').attr('id');
        });
        $('.time-picker').daterangepicker({
            timePicker: true,
            singleDatePicker: true,
            timePicker24Hour: true,
            timePickerIncrement: 1,
            locale: {
                format: 'HH:mm'
            }
        },
            (fromDate) => {
                $($inputId).val(moment(fromDate).format("HH:mm")).trigger('change');
            }).on('show.daterangepicker', (ev, picker) => {
                picker.container.find(".calendar-table").hide();
            });
    },
    dateTimePicker({ minDate = '', maxDate = '' } = {}) {
        minDate = minDate == '' ? moment().add(-10, 'y') : moment(minDate);
        maxDate = maxDate == '' ? moment().add(10, 'y') : moment(maxDate);
        var inputId = "";
        $('.date-time-picker').each((index, element) => {
            inputId = '#' + $(element).prev('input').attr('id');
            $(inputId).val(moment().format("DD-MMM-YYYY HH:mm")).trigger('change');
        });
        $('.date-time-picker').on('click', (e) => {
            inputId = '#' + $(e.currentTarget).prev('input').attr('id');
            DateTime.pickerFunction = () => { }
        });
        $('.date-time-picker').daterangepicker(
            {
                showDropdowns: true,
                startDate: minDate,
                minDate: minDate,
                maxDate: maxDate,
                endDate: maxDate,
                singleDatePicker: true,
                timePicker: true,
                autoUpdateInput: false,
                timePicker24Hour: true,
            },
            (fromDate) => {
                $(inputId).val(moment(fromDate).format("DD-MMM-YYYY HH:mm")).trigger('change');
                DateTime.pickerFunction();
            }
        ).on('showCalendar.daterangepicker', (ev, picker) => {
            if (picker.element.offset().top + picker.container.outerHeight() > $(window).height()) {
                picker.drops = 'up';
            }
            else {
                picker.drops = 'down';
            }
        }
        );
        $('.date-time-picker').on('apply.daterangepicker', (ev, picker) => {
            $(inputId).val(picker.startDate.format('DD-MMM-YYYY HH:mm'));
            DateTime.pickerFunction();
        });
    },
    dateTimePicker_current({ minDate = '', maxDate = '' }) {
        minDate = minDate == '' ? moment().add(-10, 'y') : moment(minDate);
        maxDate = maxDate == '' ? moment().add(10, 'y') : moment(maxDate);
        var inputId = "";
        $('.date-time-picker-current').each((index, element) => {
            inputId = '#' + $(element).prev('input').attr('id');
            $(inputId).val(moment().format("DD-MMM-YYYY HH:mm")).trigger('change');
        });

        $('.date-time-picker-current').on('click', (e) => {
            inputId = '#' + $(e.currentTarget).prev('input').attr('id');
            DateTime.pickerFunction = () => { }
        });
        $('.date-time-picker-current').daterangepicker({
            showDropdowns: true,
            endDate: maxDate,
            maxDate: maxDate,
            autoApply: true,
            singleDatePicker: true,
            timePicker: true,
            autoUpdateInput: false,
            timePicker24Hour: true,
            locale: {
                format: 'DD-MMM-YYYY HH:mm',
                separator: "|",
                firstDay: 1
            },
            linkedCalendars: true
        },
            (fromDate) => {
                $(inputId).val(moment(fromDate).format("DD-MMM-YYYY HH:mm")).trigger('change');
                DateTime.pickerFunction();
            }).on('showCalendar.daterangepicker', (ev, picker) => {
                if (picker.element.offset().top + picker.container.outerHeight() > $(window).height()) {
                    picker.drops = 'up';
                }
                else {
                    picker.drops = 'down';
                }
            });
        $('.date-time-picker-current').on('apply.daterangepicker', (ev, picker) => {
            $($inputId).val(picker.startDate.format('DD-MMM-YYYY HH:mm'));
            DateTime.pickerFunction();
        });
    },
    dateRangePicker() {
        var inputId = "";
        $('.date-range-picker').each((index, element) => {
            inputId = '#' + $(element).prev('input').attr('id');
            $(inputId).val(moment().startOf('month').format("DD-MMM-YYYY") + " | " + moment().endOf('month').format("DD-MMM-YYYY"));
        });

        $('.date-range-picker').on('click', (e) => {
            inputId = '#' + $(e.currentTarget).prev('input').attr('id');
        });

        $('.date-range-picker').daterangepicker({
            showDropdowns: true,
            autoApply: true,
            minDate: moment().add(-60, 'y'),
            autoUpdateInput: false,
            ranges: {
                "This Month": [moment().startOf('month'), moment().endOf('month')],
                "Last Month": [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                "Last 30 Days": [moment().subtract(30, 'days'), moment()],
                "Last 90 Days": [moment().subtract(90, 'days'), moment()],
                "This Year": [moment().startOf('year'), moment().endOf('year')]
            },
            locale: {
                format: 'DD-MMM-YYYY',
                separator: "|",
                applyLabel: "Apply",
                cancelLabel: "Cancel",
                fromLabel: "From",
                toLabel: "To",
                customRangeLabel: "Custom",
                daysOfWeek: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"],
                monthNames: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                firstDay: 1
            },
            linkedCalendars: true
        },
            (fromDate, toDate) => {
                $(inputId).val(moment(fromDate).format("DD-MMM-YYYY") + " | " + moment(toDate).format("DD-MMM-YYYY"));
                DateTime.pickerFunction();
            }).on('showCalendar.daterangepicker', (ev, picker) => {
                //if (picker.element.offset().top + picker.container.outerHeight() > $(window).height()) {
                //    picker.drops = 'up';
                //}
                //else {
                //    picker.drops = 'down';
                //}
            });
    },
    dateRangeTimePicker() {
        var $inputId = "";
        $('.date-time-range-picker').each((index, element) => {
            $inputId = '#' + $(element).parent().prev('input').attr('id');
            $($inputId).val(moment().startOf('month').format("DD-MMM-YYYY HH:mm") + " | " + moment().endOf('month').format("DD-MMM-YYYY HH:mm"));
        });

        $('.date-time-range-picker').on('click', (e) => {
            $inputId = '#' + $(e.currentTarget).parent().prev('input').attr('id');
        });

        $('.date-time-range-picker').daterangepicker({
            showDropdowns: true,
            autoApply: true,
            timePicker: true,
            minDate: moment().add(-60, 'y'),
            autoUpdateInput: false,
            ranges: {
                "This Month": [moment().startOf('month'), moment().endOf('month')],
                "Last Month": [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                "Last 30 Days": [moment().subtract(30, 'days'), moment()],
                "Last 90 Days": [moment().subtract(90, 'days'), moment()],
                "This Year": [moment().startOf('year'), moment().endOf('year')]
            },
            locale: {
                format: 'DD-MMM-YYYY HH:mm',
                separator: "|",
                applyLabel: "Apply",
                cancelLabel: "Cancel",
                fromLabel: "From",
                toLabel: "To",
                customRangeLabel: "Custom",
                daysOfWeek: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"],
                monthNames: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                firstDay: 1
            },
            linkedCalendars: true
        },
            function (fromDate, toDate) {
                $($inputId).val(moment(fromDate).format("DD-MMM-YYYY HH:mm") + " | " + moment(toDate).format("DD-MMM-YYYY HH:mm"));
                pickerFunction();
            }).on('showCalendar.daterangepicker', function (ev, picker) {
                if (picker.element.offset().top + picker.container.outerHeight() > $(window).height()) {
                    picker.drops = 'up';
                }
                else {
                    picker.drops = 'down';
                }
            });
    },
    json(date) {
        if (date == null) {
            return null;
        }
        date = date.trim();
        if (Field.isNullOrEmpty(date)) {
            return null;
        }
        switch (date.split('-')[1]) {
            case "Jan":
            case "Feb":
            case "Mar":
            case "Apr":
            case "May":
            case "Jun":
            case "Jul":
            case "Aug":
            case "Sep":
            case "Oct":
            case "Nov":
            case "Dec":
                break;
            default:
                return null;
                break;
        }
        return new Date(date.split('-')[2] + '-' + moment().month(date.split('-')[1]).format("MM") + '-' + date.split('-')[0]).toISOString();
    },
    isDefault: (date) => { return date == '0001-01-01T00:00:00' ? true : false; },
    date: (date, isDefault = false) => {
        return Field.isNullOrEmpty(date) && !isDefault ? '' : Field.isNullOrEmpty(date) && isDefault ? moment('0001-01-01T00:00:00').format('DD-MMM-YYYY') : moment(date).format('DD-MMM-YYYY');
    },
    dateTime: (date, isDefault = false) => {
        return Field.isNullOrEmpty(date) && !isDefault ?
            '' :
            Field.isNullOrEmpty(date) && isDefault ?
                moment('0001-01-01T00:00:00').format('DD-MMM-YYYY HH:mm:ss') :
                moment(date).format('DD-MMM-YYYY HH:mm:ss');
    },
    time: (date, isDefault = false) => {
        return Field.isNullOrEmpty(date) && !isDefault ?
            '' :
            Field.isNullOrEmpty(date) && isDefault ?
                moment('0001-01-01T00:00:00').format('HH:mm:ss') :
                moment(date).format('HH:mm:ss');
    },
    isValid: (value) => {
        return value != null && value != "" && isNaN(value) && !isNaN(Date.parse(value));
    },
}
const Table = {
    add({
        id = '',
        data = [],
        search = true,
        isPrint = false,
        toggle = true,
        selectPick = false,
        action = 'destroy',
        mobileResponsive = true,
        detailFormatter = null,
        overflow = 'auto',
        height = '68vh',
        fontSize = 11,
        filterControl = false,
        showfooter = false,
        thead = [],
        tbody = [],
        autoThead = false,
        removeExtraColumns = [],
        actionEvents = false,
        printTitle = '',
        reportDesc = '',
        viewMode = ''

    }) {

        if (id == '') {
            console.warn('Table id did not find...');
            return;
        }
        if ($(id).attr('data-detail-formatter')) {
            detailFormatter = $(id).attr('data-detail-formatter');
        }

        if (action == 'destroy') {
            $(id).bootstrapTable(action);
            if (thead.length > 0) {
                $(`${id} thead`).html(thead.join(''));
            }
            if (autoThead) {
                Table.autoThead({ id: id, data: data, removeCol: removeExtraColumns, actionEvent: actionEvents });
            }
            if (tbody.length > 0) {
                $(`${id} tbody`).html(pram.tbody.join(''));
                $(id).bootstrapTable({ search: pram.search, trimOnSearch: false });
            }
            else {
                $(id).bootstrapTable({
                    data: data,
                    filterControl: filterControl,
                    search: search,
                    showfooter: showfooter,
                    trimOnSearch: false,
                    detailView: detailFormatter != null ? true : false,
                    detailFormatter: detailFormatter,
                });
            }
        }
        else {
            $(id).bootstrapTable(action, data);
        }
        //Close Drop down menu if scrolled the table body
        //$('div.fixed-table-body').on('scroll', (event) => {
        //    $(event.currentTarget).find('.dropdown-menu.show').removeClass('show');
        //});
        //Table height        
        $(id).parent().css({
            "overflow-y": overflow,
            "overflow-x": overflow,
            "max-height": height,
        });
        //Font size
        $(id).css({ 'font-size': fontSize });
        //Set Responsive
        if (mobileResponsive) {
            $(id).addClass('table-mobile-responsive');
            $(`${id} thead tr:not(.hide) th:not([colspan])`).each((index, element) => {
                if ($(element).attr('colspan') === undefined) {
                    var thText = $(element).text().trim();
                    $(`${id} tbody tr td:nth-child(${(index + 1)})`).attr('data-title', thText);
                }
            });
        }

        //Set Check As Toggle Botton
        if (toggle) {
            $(`${id} input[type="checkbox"]`).parent('label').parent('div.th-inner').addClass('p-0');
            $(`${id} input[type="checkbox"]`).parent('label').addClass('form-switch ps-0 pt-1');
            $(`${id} input[type="checkbox"]`).addClass('form-check-input');
        }
        if (viewMode == "View" || viewMode == "view") {
            $(`${id} input`).attr('disabled', true);
            $(`${id} textarea`).attr('disabled', true);
            $(`${id} select`).attr('disabled', true);
        }
        //Set Select As Select Pick
        if (selectPick) {
            $(`${pram.id} tbody select.select-pick[data-isMatched="false"]`).val(null);
            Dropdown.refresh({ selector: `${pram.id} tbody select.select-pick` });
        }
        //Print
        if (isPrint) {
            let obj = App.Company;
            let table = document.querySelector(id).cloneNode(true);
            if (obj != undefined) {
                let noOfCol = 0;
                table.tHead.querySelector('tr').querySelectorAll('th').forEach(th => {
                    noOfCol += parseInt(th.attributes['colspan'] != undefined ? th.attributes['colspan'].value : 1);
                });
                let address = [obj.Address1, obj.PostOffice, obj.District, obj.StateName, obj.PinCode];
                let title = `
                <tr>
                    <th colspan="${noOfCol}">
                        <div class="company-name">${obj.Description}</div>
                        <div class="company-add">
                            ${address.filter(x => x != null).join(', ')}
                        </div>
                        <div class="report-desc">${reportDesc}</div>
                    </th>
                </tr>`;
                $(table.tHead).prepend(title);
            }
            var htmlString = [];
            htmlString.push('<table class="table">');
            htmlString.push($(table).html());
            htmlString.push('</table>');

            var style = [];
            style.push(".table{border-spacing: 0; width: 100%;}");
            style.push(".table th{border: 1px solid black; text-align:left; padding-left:5px; padding-right: 5px;}");
            style.push(".table td{border: 1px solid black; padding-left:5px; padding-right: 5px;}");
            style.push("td, th{font-size:14px; font-family:Calibri;}");
            style.push(".table-child{border-spacing: 0;}");
            style.push(".table-child td{border: none;}");
            style.push(".table-child th{border: none;}");
            style.push(".not-print{display:none;}");
            style.push("td.td-num{text-align:right;}");
            Print.table({ title: printTitle, style: style, content: htmlString, orientation: 'A4 landscape', print: false, tableId: '#tableSubGroup' });
        }
        //Note work on search input. for content
    },
    empty({ selector = '.table-default' }) {
        $(selector).attr('data-trim-on-search', false);
        $(selector).bootstrapTable('destroy');
        $(selector).bootstrapTable();
        //Table height        
        $(selector).parent().css({
            "overflow-y": 'auto',
            "overflow-x": 'auto',
            "max-height": '68vh'
        });
    },
    remove({ id = '', value = [-1], field = '$index', toggle = false }) {

        if (id == '') {
            console.warn('Table Id or trDataIndex is empty.');
            return;
        }
        $(id).bootstrapTable('remove', { values: value, field: field });
        //Set Select As Select Pick
        $(`${id} tbody select.select-pick[data-isMatched="false"]`).val(null);
        Dropdown.refresh({ selector: `${id} tbody select.select-pick` });
        //Set Check As Toggle Botton
        if (toggle) {
            $(`${id} input[type="checkbox"]`).parent('label').parent('div.th-inner').addClass('p-0');
            $(`${id} input[type="checkbox"]`).parent('label').addClass('form-switch ps-0 pt-1');
            $(`${id} input[type="checkbox"]`).addClass('form-check-input');
        }
    },
    updateById({ id = '', objId = -1, obj = {} }) {
        if (Field.isNullOrEmpty(id)) {
            console.warn('Table id did not find.');
            return;
        }
        if (objId == -1) {
            console.warn('data id did not find.');
            return;
        }
        if ($.isEmptyObject(obj)) {
            console.warn('value is empty.');
            return;
        }
        $(id).bootstrapTable('updateByUniqueId', { id: objId, row: obj });
    },
    updateByIndex({ id = '', index = -1, obj = {}, value = null, event = null, toggle = false }) {
        if (Field.isNullOrEmpty(id)) {
            console.warn('Table id did not find.');
            return;
        }
        if (index == -1) {
            console.warn('index no. did not find.');
            return;
        }
        if ($.isEmptyObject(obj)) {
            console.warn('value is empty.');
            return;
        }
        //Get Text Cursor Poition
        let curStart = event == null ? -1 : document.getElementById(pram.event.target.id).selectionStart;
        //Update Table By Index
        $(id).bootstrapTable('updateRow', { index: index, row: obj });
        //Set Select As Select Pick
        $(`${id} tbody select.select-pick[data-isMatched="false"]`).val(null);
        Dropdown.refresh({ selector: `${id} tbody select.select-pick` });
        //Set Check As Toggle Botton
        if (toggle) {
            $(`${id} input[type="checkbox"]`).parent('label').parent('div.th-inner').addClass('p-0');
            $(`${id} input[type="checkbox"]`).parent('label').addClass('form-switch ps-0 pt-1');
            $(`${id} input[type="checkbox"]`).addClass('form-check-input');
        }
        //Set Cursor Position and focus
        if (event != null) {
            var field = document.getElementById($(event.target).prop('id'));
            if (event.currentTarget.tagName.toLowerCase() == "select") {
                if (Array.from(event.currentTarget.classList).filter(value => value == 'select-pick').length > 0) {
                    $(`button[data-target="#${field.id}-SelectPick-Menu"]`).trigger('focus');
                }
                else {
                    $(`#${field.id}`).trigger('focus');
                }
            }
            else if (value != null && curStart > -1) {
                var curPos = curStart > 0 && curStart < value.length ? curStart : value.length;
                if (field.type == "date") {
                    $(`#${field.id}`).trigger('focus').val('').val(value);
                }
                else {
                    field.setSelectionRange(curPos, curPos);
                    field.focus();
                }
            }
        }
    },
    autoThead({ id = '', data = [], removeCol = [], actionEvent = false }) {
        var thead = [];
        thead.push('<tr>');
        $.map(data[0], (value, fieldName) => {
            var dataField = fieldName;
            var theadText = fieldName.replaceAll('_', ' ').replace(/([A-Z])/g, ' $1').replace(/\s\s+/g, ' ').toCamelCase();
            if (removeCol.indexOf(dataField) == -1) {
                if (DateTime.isValid(value)) {
                    thead.push(`<th data-field="${dataField}" data-formatter="${id.replace("#", "")}Date" class="text-nowrap">${theadText}</th>`);
                }
                else if (fieldName == "Status") {
                    thead.push(`<th data-field="${dataField}" data-formatter="${id.replace("#", "")}Status">${theadText}</th>`);
                }
                else {
                    thead.push(`<th data-field="${dataField}">${theadText}</th>`);
                }
            }
        });
        if (actionEvent == true) {
            thead.push(`
                <th data-formatter="${id.replace('#', '')}Action" data-events="${id.replace('#', '')}Event" class="not-print text-center">
                    <i class="fa-solid fa-list-check"></i>
                </th>
            `);
        }
        thead.push('</tr>');
        $(`${id} thead`).html(thead.join(''));
    },
    setCursor({ event = null, value = null, cursorStart = 0, callBack = () => { } }) {
        callBack();
        if (event != null && value != null) {
            var field = document.getElementById($(event.target).prop('id'));
            var cursorPosition = cursorStart > 0 && cursorStart < value.length ? cursorStart : value.length;
            if (field.type == "date") {
                $('#' + field.id).trigger('focus').val('').val(value);
            }
            else {
                field.setSelectionRange(cursorPosition, cursorPosition);
                field.focus();
            }
        }
    },
    addtree({
        id = '',
        data = [],
        filterControl = false,
        showFullscreen = false,
        showExport = false,
        search = false,
        mobileResponsive = false,
        showPrint = false,
        idField = '',
        treeShowField = '',
        parentIdField = '',
        treeColumn = 1,
        oprationType = '',
        heightStatus = '',
        height = ''
    }) {
        if (id == '') {
            console.warn('Table id did not find. So data has not bind in table.');
            return;
        }
        if (data.length == 0 || data == null) {
            $(tableId).bootstrapTable(oprationType);
            $(tableId).bootstrapTable();
            console.warn('Bs Table Data not found');
            return;
        }
        $(id).bootstrapTable(oprationType);
        $(id).bootstrapTable({
            filterControl: filterControl,
            data: data,
            showFullscreen: showFullscreen,
            showExport: showExport,
            search: search,
            mobileResponsive: mobileResponsive,
            showPrint: showPrint,
            idField: idField,
            treeShowField: treeShowField,
            parentIdField: parentIdField,
            onPostBody: function () {
                $(id).treegrid({
                    treeColumn: treeColumn,
                    expanderExpandedClass: 'fa fa-minus-circle text-primary',
                    expanderCollapsedClass: 'fa fa-plus-circle text-primary',
                    initialState: 'collapsed',
                    onChange: function () {
                        $(id).bootstrapTable('resetView')
                    }
                })
            },
            printPageBuilder: function (pram) {
                return '<html><head><style type="text/css" media="print"> @page { size: auto; margin: 25px 0 25px 0; } </style> <style type="text/css" media="all"> table { border-collapse: collapse; font-size: 12px; } table, th, td { border: 1px solid grey; } th, td { text-align: center; vertical-align: middle; } p { font-weight: bold; margin-left:20px; } table { width:94%; margin-left:3%; margin-right:3%; } div.bs-table-print { text-align:center; } </style> </head><title>' + printTitle + '</title> <body> <p>'.concat(printTitle).concat('<p> <div class="bs-table-print">').concat(pram).concat('</div> </body> </html>')
            }
        });
        $(id).parent().css({
            "overflow-y": "auto",
            "overflow-x": "auto",
            "max-height": height
        });
        $(id).css({ 'font-size': fontSize });
        if (mobileResponsive) {
            $(id).addClass('table-mobile-responsive');
            $(id + ' thead tr:not(.hide) th').each(function (index) {
                var thText = $(this).text().trim();
                $(id + ' tbody tr td:nth-child(' + (index + 1) + ')').attr('data-title', thText);
            });
        }
    },
}
const Dropdown = {
    bind({
        id = '',
        data = [],
        value = 'Id',
        text = 'Text',        
        icon = '',
        subText = '',
        html = '',
        initialValue = [],
        json = false,        
        isSelectPick = true,
        isEditable = false,
        disabled
    }) {
        if (isEditable) {
            $(id).editableSelect('destroy');
        }
        let select = document.querySelector(id);
        if (select == null) {
            console.warn(`Select Id (${id}) is undefined`);
            return;
        }

        select.disabled = disabled === undefined ? select.disabled : disabled;
        select.innerHTML = "";
        const fragment = document.createDocumentFragment();
        let isOptionMatched = false;
        data.forEach((obj, index) => {
            let option = new Option();
            option.value = obj[value];
            option.selected = initialValue.map(value => value == null ? null : value.toString()).includes(option.value);
            isOptionMatched = isOptionMatched ? true : initialValue.map(value => value == null ? null : value.toString()).includes(option.value);
            option.text = obj[text];
            if (obj[icon]) {
                option.setAttribute(SELECTPICK_ITEM_DATA_ICON, obj[icon]);
            }
            if (obj[subText]) {
                option.setAttribute(SELECTPICK_ITEM_DATA_SUBTEXT, obj[subText]);
            }
            if (obj[html]) {
                option.setAttribute(SELECTPICK_ITEM_DATA_HTML, obj[html]);
            }
            if (json) {
                option.setAttribute('data-json', JSON.stringify(obj));
            }
            fragment.appendChild(option);
        });
        select.appendChild(fragment);
        if (!isOptionMatched) {
            select.value = null;
        }
        if (isEditable) {
            $(id).editableSelect({ effects: 'slide' });
        }
        else if (isSelectPick) {
            SelectPick.render(select);
        }
    },
    bindIcon({ id = '', value = [] }) {
        if (Field.isNullOrEmpty(id)) {
            console.warn('id did not find for bind icon.')
            return;
        }
        Data.get(
            {
                url: "/lib/site/fontawesome.json",
                isApi: false,
                onSuccess: (response) => {
                    Dropdown.bind({ id: id, data: response.data, value: 'Value', text: 'Value', icon: 'Value', initialValue: value });
                }
            }
        );
    },
    set({ id = '', value = null, text = null }) {
        if (!id) {
            Message.show({ statusText: 'Id is unidefined' });
            return;
        }
        SelectPick.set({ id: pram.id, value: pram.value, text: pram.text });
    },
    itemJson({ id = '' }) {
        if ($(`${id} option:selected`).attr('data-json') == undefined) {
            return null;
        }
        else {
            return JSON.parse($(`${id} option:selected`).attr('data-json'));
        }
    },
    refresh({ selector = '', isDisabled }) {
        if (Field.isNullOrEmpty(selector)) {
            console.warn('Selector is undefined or empty in the function Dropdown refresh.');
            return;
        }
        if (isDisabled != undefined) {
            $(pram.selector).attr('disabled', isDisabled);
        }
        SelectPick.refresh({ selector: selector });
    },
    html({
        id = '',
        className = '',
        data = [],
        value = '',
        text = '',
        size = 4,
        icon = '',
        subText = '',
        html = '',
        initialValue = [],
        json = false,
        title = 'Nothing Selected',
        selectionType = '',
        parent = '',
        addFn = '',
        editFn = ''
    }) {
        let selectHtml = [];
        if (Field.isNullOrEmpty(id)) {
            return selectHtml.join('');
        }
        let isMatched = data.filter(obj => initialValue.toString().includes(obj[value] == null ? "" : obj[value].toString())).length > 0 ? true : false;
        selectHtml.push(`<select id="${id}" class="select-pick ${className}" title="${title}" ${selectionType} data-search="true" data-size="${size}" data-parent="${parent}" data-ismatched="${isMatched}"`);
        if (addFn !== undefined) {
            selectHtml.push(` data-action="true" `);
            selectHtml.push(` data-addFn="${addFn}" `);
        }
        if (editFn !== undefined) {
            selectHtml.push(` data-editFn="${editFn}" `);
        }
        selectHtml.push(`>`);

        data.forEach((obj, index) => {
            let option = '';
            option += `<option `;
            option += `value ='${obj[value].toString().replaceAll("'", "&#39;")}' `;
            option += `${initialValue.map(value => value == null ? null : value.toString()).includes(obj[value].toString()) ? `selected` : ``} `;
            if (obj[icon]) {
                option += `${SELECTPICK_ITEM_DATA_ICON} = '${obj[icon].replaceAll("'", "&#39;")}' `;
            }
            if (obj[subText]) {
                option += `${SELECTPICK_ITEM_DATA_SUBTEXT} = '${obj[subText].replaceAll("'", "&#39;")}' `;
            }
            if (obj[html]) {
                option += `${SELECTPICK_ITEM_DATA_HTML} = '${obj[html].replaceAll("'", "&#39;")}' `;
            }
            if (json) {
                option += `data-json = '${JSON.stringify(obj).replaceAll("'", "&#39;")}' `;
            }
            option += `>`;
            option += `${obj[text]}`;
            option += `</option>`;
            selectHtml.push(option);
        });
        selectHtml.push(`</select>`);
        return selectHtml.join('');
    }
}
class _Number {
    static validate(pram = { value: 0, dp: 2, min: 0, max: 0 }) {
        let value = 0;
        //Vlidate Input value is Number or not
        if (pram.value != undefined) {
            const regex = /^[0-9.-]*$/;
            const filteredValue = Array.from(pram.value).filter(char => regex.test(char)).join('');
            value = filteredValue.replace(/^(.*?\..*?)\..*$/, '$1');
            let isNegative = value.startsWith('-');
            value = value.replaceAll('-', '');
            value = isNegative ? `-${value}` : value;
            //Validate Decimal Place
            if (pram.dp != undefined && value.includes('.')) {
                let num = value.split('.')[0].length == 0 ? '0' : value.split('.')[0];
                let decimal = value.split('.')[1];
                let decimalPlace = pram.dp;
                decimalPlace = isNaN(decimalPlace) == false ? parseInt(decimalPlace) : 0;
                decimal = decimal.length > decimalPlace ? `.${decimal.substring(0, decimalPlace)}` : decimalPlace > 0 ? `.${decimal}` : '';
                value = `${num}${decimal}`;
            }
            //Validate Max Value
            if (pram.max != undefined) {
                let max = pram.max;
                max = isNaN(max) == false ? parseInt(max) : 0;
                value = parseFloat(value) > max ? pram.max : value;
            }
            //Validate Min Value
            if (pram.min != undefined) {
                let min = pram.min;
                min = isNaN(min) == false ? parseInt(min) : 0;
                value = parseFloat(value) < min ? pram.min : value;
            }
        }
        return value;
    }
    static format(pram = { num: 0, dp: 0, currency: 'INR' }) {
        pram.num = pram.num === undefined ? 0 : pram.num;
        pram.dp = pram.dp === undefined ? 0 : pram.dp;
        pram.currency = pram.currency === undefined ? '' : pram.currency;
        let numFormat = Field.isNullOrEmpty(pram.currency) ?
            new Intl.NumberFormat('en-IN', { minimumFractionDigits: pram.dp }) :
            new Intl.NumberFormat('en-IN', { style: 'currency', currency: pram.currency, minimumFractionDigits: pram.dp });
        let formattedNum = numFormat.format(pram.num);
        if (!Field.isNullOrEmpty(pram.currency)) {
            let symbol = formattedNum.substring(0, 1);
            formattedNum = `${symbol} ${formattedNum.substring(1, formattedNum.length)}`;
        }
        return formattedNum;
    }
    static toIndianWord(number) {
        number = number.toString();
        var sglDigit = ["Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"],
            dblDigit = ["Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"],
            tensPlace = ["", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"],
            handle_tens = function (dgt, prevDgt) {
                return 0 == dgt ? "" : " " + (1 == dgt ? dblDigit[prevDgt] : tensPlace[dgt])
            },
            handle_utlc = function (dgt, nxtDgt, denom) {
                return (0 != dgt && 1 != nxtDgt ? " " + sglDigit[dgt] : "") + (0 != nxtDgt || dgt > 0 ? " " + denom : "")
            };

        var str = "",
            digitIdx = 0,
            digit = 0,
            nxtDigit = 0,
            words = [];
        var re = /(0|([1-9]\d*))(\.\d+)?/g
        if (re.test(number)) {
            var arr = number.split('.');
            number = arr[0];
            for (digitIdx = number.length - 1; digitIdx >= 0; digitIdx--) {
                switch (digit = number[digitIdx] - 0, nxtDigit = digitIdx > 0 ? number[digitIdx - 1] - 0 : 0, number.length - digitIdx - 1) {
                    case 0:
                        words.push(handle_utlc(digit, nxtDigit, ""));
                        break;
                    case 1:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;
                    case 2:
                        words.push(0 != digit ? " " + sglDigit[digit] + " Hundred" + (0 != number[digitIdx + 1] && 0 != number[digitIdx + 2] ? " and" : "") : "");
                        break;
                    case 3:
                        words.push(handle_utlc(digit, nxtDigit, "Thousand"));
                        break;
                    case 4:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;
                    case 5:
                        words.push(handle_utlc(digit, nxtDigit, "Lakh"));
                        break;
                    case 6:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;
                    case 7:
                        words.push(handle_utlc(digit, nxtDigit, "Crore"));
                        break;
                    case 8:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;

                    case 9:
                        words.push(handle_utlc(digit, nxtDigit, "Arab"));
                        break;
                    case 10:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;

                    case 11:
                        words.push(handle_utlc(digit, nxtDigit, "Kharab"));
                        break;
                    case 12:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;

                    case 13:
                        words.push(handle_utlc(digit, nxtDigit, "Nil"));
                        break;
                    case 14:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;

                    case 15:
                        words.push(handle_utlc(digit, nxtDigit, "Padma"));
                        break;
                    case 16:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;

                    case 17:
                        words.push(handle_utlc(digit, nxtDigit, "Shankh"));
                        break;
                    case 18:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;

                    case 19:
                        words.push(0 != digit ? " " + sglDigit[digit] + " Hundred" + (0 != number[digitIdx + 1] && 0 != number[digitIdx + 2] ? " and" : "") : "");
                        break;
                    case 20:
                        words.push(handle_utlc(digit, nxtDigit, "Thousand"));
                        break;
                    case 21:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;
                    case 22:
                        words.push(handle_utlc(digit, nxtDigit, "Lakh"));
                        break;
                    case 23:
                        words.push(handle_tens(digit, number[digitIdx + 1]));
                        break;
                }
            }
            str = "Rupees" + words.reverse().join("")
            if (arr.length > 1) {
                number = arr[1].substring(0, 3);
                words = [];
                for (digitIdx = number.length - 1; digitIdx >= 0; digitIdx--) {
                    switch (digit = number[digitIdx] - 0, nxtDigit = digitIdx > 0 ? number[digitIdx - 1] - 0 : 0, number.length - digitIdx - 1) {
                        case 0:
                            words.push(handle_utlc(digit, nxtDigit, ""));
                            break;
                        case 1:
                            words.push(handle_tens(digit, number[digitIdx + 1]));
                            break;
                        case 2:
                            words.push(0 != digit ? " " + sglDigit[digit] + "" + (0 != number[digitIdx + 1] && 0 != number[digitIdx + 2] ? "" : "") : "");
                            break;
                    }
                }
                str += number > 0 ? " and " + words.reverse().join("") + " only/-" : words.reverse().join("") + " only/-";
            }
        } else str = "";
        return str
    }
    static toInternationalWord(number) {
        var th_val = ['', 'thousand', 'million', 'billion', 'trillion'];
        var dg_val = ['zero', 'one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine'];
        var tn_val = ['ten', 'eleven', 'twelve', 'thirteen', 'fourteen', 'fifteen', 'sixteen', 'seventeen', 'eighteen', 'nineteen'];
        var tw_val = ['twenty', 'thirty', 'forty', 'fifty', 'sixty', 'seventy', 'eighty', 'ninety'];
        number = number.toString();
        //number = number.replace(/[\, ]/g, '');
        if (number != parseFloat(number))
            return 'not a number ';
        var x_val = number.indexOf('.');
        if (x_val == -1)
            x_val = number.length;
        if (x_val > 15)
            return 'too big';
        var n_val = number.split('');
        var str_val = '';
        var sk_val = 0;
        for (var i = 0; i < x_val; i++) {
            if ((x_val - i) % 3 == 2) {
                if (n_val[i] == '1') {
                    str_val += tn_val[Number(n_val[i + 1])] + ' ';
                    i++;
                    sk_val = 1;
                } else if (n_val[i] != 0) {
                    str_val += tw_val[n_val[i] - 2] + ' ';
                    sk_val = 1;
                }
            } else if (n_val[i] != 0) {
                str_val += dg_val[n_val[i]] + ' ';
                if ((x_val - i) % 3 == 0)
                    str_val += 'hundred ';
                sk_val = 1;
            }
            if ((x_val - i) % 3 == 1) {
                if (sk_val)
                    str_val += th_val[(x_val - i - 1) / 3] + ' ';
                sk_val = 0;
            }
        }
        if (x_val != number.length) {
            var y_val = number.length;
            (y_val - (x_val + 1)) > 3 ? y_val = number.length - 1 : y_val = number.length;
            parseInt(number.substring(x_val + 1, number.length)) != 0 ? str_val += 'and ' : '';
            for (var i = x_val + 1; i < y_val; i++) {
                if ((y_val - i) % 3 == 2) {
                    if (n_val[i] == '1') {
                        str_val += tn_val[Number(n_val[i + 1])] + ' ';
                        i++;
                        sk_val = 1;
                    } else if (n_val[i] != 0) {
                        str_val += tw_val[n_val[i] - 2] + ' ';
                        sk_val = 1;
                    }
                } else if (n_val[i] != 0) {
                    str_val += dg_val[n_val[i]] + ' ';
                    if ((y_val - i) % 3 == 0)
                        //str_val += 'hundred ';
                        sk_val = 1;
                }
                if ((y_val - i) % 3 == 1) {
                    if (sk_val)
                        str_val += th_val[(y_val - i - 1) / 3] + ' ';
                    sk_val = 0;
                }
            }
        }
        str_val += 'only';
        return (str_val.replace(/\s+/g, ' ')).toUpperCase().trim();
    }

    static toIndianNumber(number, decimalPlace = 2) {
        let rupee = new Intl.NumberFormat('en-IN', {
            style: 'currency',
            currency: 'INR',
            minimumFractionDigits: decimalPlace
        });
        return rupee.format(number).toString().replace("₹", "");
    }
    static toIndianCurrency(number, decimalPlace = 2) {
        let rupee = new Intl.NumberFormat('en-IN', {
            style: 'currency',
            currency: 'INR',
            minimumFractionDigits: decimalPlace
        });
        return rupee.format(number).replace("₹", "₹ ");
    }
    static validate1(value, min, max, decimalPlace) {
        try {
            var sign = "";
            if (value == '-') {
                return value;
            }
            if (value.includes('-')) {
                sign = "-";
                value = value.toString().split('-').length > 1 ? value.toString().split('-')[1] : value;
            }
            if (value == '') {
                return sign + 0;
            }
            if (isNaN(value)) {
                return sign + 0;
            }

            var beforeDecimal = Number(value.toString().split('.')[0]);
            if (decimalPlace > 0) {
                var afterDecimal = typeof (value.toString().split('.')[1]) == 'undefined' ? '' : value.toString().split('.')[1] == '' ? '.' : '.' + value.toString().split('.')[1];
                if (afterDecimal.length > decimalPlace + 1) {
                    value = beforeDecimal + afterDecimal.substring(0, decimalPlace + 1);
                }
                else {
                    value = beforeDecimal + afterDecimal;
                }
            }
            else {
                value = beforeDecimal.toString().replace('.');
            }

            if (min != null && parseFloat(value) < min) {
                return min;
            }
            if (max != null && parseFloat(value) > max) {
                return min;
            }
            return sign + value.replaceAll('-', '');
        }
        catch (ex) {
            console.log(ex);
            return 0;
        }

    }
}

String.prototype.toCamelCase = function () {
    var lcStr = this.toLowerCase();
    return lcStr.replace(/(?:^|\s)\w/g, function (match) {
        return match.toUpperCase();
    });
}