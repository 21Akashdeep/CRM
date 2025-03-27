
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
        document.cookie = `${name}=${value};expires=${expires};path=/`;
    },
    remove(name) {
        document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/`;
    }
};

const SessionStorage = {
    set(key, Value) {
        sessionStorage.setItem(key, Value);
    },
    get(key) {
        return sessionStorage.getItem(key);
    },
    remove(key) {
        sessionStorage.removeItem(key);
    },
    removeAll() {
        sessionStorage.clear();
    },
}

const App = {
    Setting: SessionStorage.get('Setting') ? JSON.parse(SessionStorage.get('Setting')) : null,
    User: SessionStorage.get('User') ? JSON.parse(SessionStorage.get('User')) : null,
    AppMenu: SessionStorage.get('AppMenu') ? SessionStorage.get('AppMenu') : '',
}
//Object.freeze(App.Setting.Info, App.Setting.UserType, App.Setting.ApiType, App.Setting.ApiName, App.User, App.Permission);

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
            timer: Message.timer
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
    get({ url = null, loader = true, async = false, isApi = true, onSuccess = () => { } }) {
        const authToken = SessionStorage.get('User') ? `Bearer ${JSON.parse(SessionStorage.get('User')).AuthToken}` : '';
        const apiUrl = isApi ? Url.Api + url : Url.App + url;
        $.ajax({
            url: apiUrl,
            type: 'Get',
            async: async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            cache: false,
            headers: {
                Authorization: authToken,
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
    post({ url = null, data = null, loader = true, loaderName = '', async = false, isApi = true, onSuccess = () => { } }) {
        const authToken = SessionStorage.get('User') ? `Bearer ${JSON.parse(SessionStorage.get('User')).AuthToken}` : '';
        const apiUrl = isApi ? Url.Api + url : Url.App + url;

        $.ajax({
            url: apiUrl,
            type: 'POST',
            async: async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            headers: { Authorization: authToken },
            data: JSON.stringify(data),
            beforeSend: () => {
                if (loader) {
                    if (loaderName) {
                        $(loaderName).show(); // Show loader if loaderName is provided
                    } else {
                        PageLoader.on(); // Otherwise, use the default PageLoader
                    }
                }
            },
            success: (response) => {
                // Handle redirect or unauthorized response
                if ([Message.Type.redirect, Message.Type.unauthorized].includes(response.status)) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App + response.redirectPage);
                    }, 3000);
                    return;
                }
                // Call the onSuccess callback
                try {
                    onSuccess(response);
                } catch (ex) {
                    console.error(ex);
                }
            },
            error: (response) => {
                // Handle unauthorized error
                if (response.status === Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => {
                        location.replace(Url.App); // Redirect to login if unauthorized
                    }, 3000);
                    return;
                }

                // Log the error and show the message
                console.error(JSON.stringify(response));
                Message.show(response);
            },
            complete: () => {
                if (loader) {
                    if (loaderName) {
                        $(loaderName).hide(); // Hide loader if loaderName was used
                    } else {
                        PageLoader.off(); // Otherwise, hide the default PageLoader
                    }
                }
            }
        });
    },
    update({ url = null, data = null, loader = true, async = false, isApi = true, onSuccess = () => { } }) {
        const authToken = SessionStorage.get('User') ? `Bearer ${JSON.parse(SessionStorage.get('User')).AuthToken}` : '';
        const apiUrl = isApi ? Url.Api + url : Url.App + url;
        $.ajax({
            url: apiUrl,
            type: 'PATCH',
            async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            headers: { Authorization: authToken },
            data: JSON.stringify(data),
            beforeSend: loader ? PageLoader.on : null,
            success: (response) => {
                if ([Message.Type.redirect, Message.Type.unauthorized].includes(response.status)) {
                    Message.alert(response);
                    setTimeout(() => location.replace(Url.App + response.redirectPage), 3000);
                    return;
                }

                try {
                    onSuccess(response);
                } catch (ex) {
                    console.error(ex);
                }
            },
            error: (response) => {
                if (response.status === Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => location.replace(Url.App), 3000);
                    return;
                }
                console.error(response);
                Message.show(response);
            },
            complete: () => loader && PageLoader.off(),
        });
    },
    delete({ url = null, loader = true, async = false, isApi = true, onSuccess = () => { } }) {
        const authToken = SessionStorage.get('User') ? `Bearer ${JSON.parse(SessionStorage.get('User')).AuthToken}` : '';
        const apiUrl = isApi ? Url.Api + url : Url.App + url;
        $.ajax({
            url: apiUrl,
            type: 'DELETE',
            async,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            cache: false,
            headers: { Authorization: authToken },
            beforeSend: loader ? PageLoader.on : null, // Show loader if requested
            success: (response) => {
                if ([Message.Type.redirect, Message.Type.unauthorized].includes(response.status)) {
                    Message.alert(response);
                    setTimeout(() => location.replace(Url.App + response.redirectPage), 3000);
                    return;
                }

                try {
                    onSuccess(response); // Call the success callback
                } catch (ex) {
                    console.error(ex);
                }
            },
            error: (response) => {
                if (response.status === Message.Type.unauthorized) {
                    Message.alert(response);
                    setTimeout(() => location.replace(Url.App), 3000); // Redirect to login page if unauthorized
                    return;
                }

                console.error(response); // Log other errors
                Message.show(response); // Show error message
            },
            complete: () => loader && PageLoader.off(), // Hide loader after request completes
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
    base64ToBytes(base64) {
        const binaryString = atob(base64);
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        Array.from(binaryString).forEach((char, i) => {
            bytes[i] = char.charCodeAt(0);
        });
        return bytes.buffer;
    },
    attachFile({ objFile = [], AllowedExtension = ['png', 'jpg', 'jpeg', 'pdf'], afterReaderOnLoad = () => { } }) {
        const file = objFile?.files[0]; // Corrected: Changed `pram` to `objFile`
        if (!file) {
            Message.error({ statusText: 'File did not attach.' });
            return;
        }

        const fileExt = file.name.split('.').pop();
        const elementId = `#${$(objFile).parent().next('.file-preview').find('iframe').attr('id')}`; // Corrected: Changed `pram` to `objFile`

        // Check for allowed file extensions
        if (!AllowedExtension.includes(fileExt)) {  // Corrected: Changed `pram` to `AllowedExtension`
            Data.setImageInIframe({ IframeId: elementId, src: "/images/uploadlogo.png" });
            Message.Error({ statusText: 'Only attach png, jpg, jpeg or pdf.' });
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            Data.setIframe({ IframeId: elementId, src: e.target.result });
            $(elementId).hide().fadeIn(650);
            const [mimeType, base64] = e.target.result.split(',');
            afterReaderOnLoad({ MimeType: mimeType, Base64: base64 });  // Corrected: Changed `pram` to `afterReaderOnLoad`
        };
        reader.readAsDataURL(file);
    },
    setIframe({ IframeId = '', src = '' }) {
        if (!IframeId || !src) {
            console.warn("IFrame id or source is empty...");
            return;
        }
        const iframe = $(IframeId);
        if (src.startsWith('data:application/pdf')) {
            iframe.removeAttr('srcdoc').attr('src', src);
        } else {
            const height = iframe.height() - 23;
            const width = iframe.width() - 23;
            const htmlString = `<html><body style="text-align: center;"><img class="border" src="${src}" height="${height}" width="${width}"/></body></html>`;
            iframe.attr('src', src).attr('srcdoc', htmlString);
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

        Data.setIframe({ IframeId: '#' + $(pram.id + ' iframe').attr('id'), src: "/images/uploadlogo.png" });

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
            /*$(pram.id + ' .modal-footer button').html('<span class="fa fa-edit"></span>');*/
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
        Data.setIframe({ IframeId: $(pram.id + ' .file-upload iframe').attr('id'), src: "/images/uploadlogo.png" });
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
        Data.setIframe({ IframeId: $(pram.id + ' .file-upload iframe').attr('id'), src: "/images/uploadlogo.png" });
        $(pram.id).modal('hide');
    },
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
                @page {
                    size:${orientation}; margin:3mm; counter-increment:page; counter-reset:page 1; 
                    @bottom-right { content: "Page " counter(page) " of " counter(pages)};
                }
                table { page-break-after: auto; border-collapse: collapse; width:100%; }                
                thead { display: table-header-group; }
                tfoot { display: table-footer-group; }
                tr { page-break-inside: avoid; page-break-after: auto; }
                td { page-break-inside: avoid; page-break-after: auto; border: ${border ? `1px solid black;` : `none;`}}
                th { page-break-inside: avoid; page-break-after: auto; border: ${border ? `1px solid black;` : `none;`}}                                
                .btn-print { display: none; }
            }
            td.td-num { vertical-align:middle; text-align: right; white-space: nowrap; }
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
            .btn-print:hover { color:#fff; border-color: #212529; border-radius:5px; background-color:#212529}            
            .th-heading { font-size:14px;}
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
                    <br/>
                    <button type="button" class="btn-print text-center" onClick="window.print(); window.close();">
                        <span class="fa fa-print"></span>&nbsp;&nbsp;Print
                    </button>
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
class DateTime {
    static init() {
        DateTime.monthPicker();
        DateTime.datePicker();
        DateTime.dateTimePicker();
        DateTime.timePicker();
        DateTime.dateRangePicker();
        DateTime.dateRangeTimePicker();
    }
    static pickerFunction() { }
    static monthPicker() {
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
    };
    static datePicker(pram = { minDate: '', maxDate: '' }) {
        pram.minDate = pram.minDate === undefined ? moment().add(-10, 'y') : pram.minDate;
        pram.maxDate = pram.maxDate === undefined ? moment().add(10, 'y') : pram.maxDate;
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
            minDate: pram.minDate,
            maxDate: pram.maxDate,
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
    };
    static timePicker() {
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
    };
    static dateTimePicker(pram = { minDate: '', maxDate: '' }) {
        pram.minDate = typeof (pram.minDate) == 'undefined' || pram.minDate == '' ? moment().add(-10, 'y') : moment(pram.minDate);
        pram.maxDate = typeof (pram.maxDate) == 'undefined' || pram.maxDate == '' ? moment().add(10, 'y') : moment(pram.maxDate);
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
                startDate: pram.minDate,
                minDate: pram.minDate,
                maxDate: pram.maxDate,
                endDate: pram.maxDate,
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
    }
    static dateTimePicker_current(pram = { minDate: '', maxDate: '' }) {
        pram.minDate = typeof (pram.minDate) == 'undefined' || pram.minDate == '' ? moment().add(-10, 'y') : moment(pram.minDate);
        pram.maxDate = typeof (pram.maxDate) == 'undefined' || pram.maxDate == '' ? moment().add(10, 'y') : moment(pram.maxDate);
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
            /*minDate: pram.minDate,*/
            endDate: pram.maxDate,
            maxDate: pram.maxDate,
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
    }
    static dateRangePicker() {
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
    }
    static dateRangeTimePicker() {
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
    }
    static json(date) {
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
    }
    static isDefault = (date) => { return date == '0001-01-01T00:00:00' ? true : false; }
    static date = (date, isDefault = false) => {
        return Field.isNullOrEmpty(date) && !isDefault ?
            '' :
            Field.isNullOrEmpty(date) && isDefault ?
                moment('0001-01-01T00:00:00').format('DD-MMM-YYYY') :
                moment(date).format('DD-MMM-YYYY');
    }
    static dateTime = (date, isDefault = false) => {
        return Field.isNullOrEmpty(date) && !isDefault ?
            '' :
            Field.isNullOrEmpty(date) && isDefault ?
                moment('0001-01-01T00:00:00').format('DD-MMM-YYYY HH:mm:ss') :
                moment(date).format('DD-MMM-YYYY HH:mm:ss');
    }
    static time = (date, isDefault = false) => {
        return Field.isNullOrEmpty(date) && !isDefault ?
            '' :
            Field.isNullOrEmpty(date) && isDefault ?
                moment('0001-01-01T00:00:00').format('HH:mm:ss') :
                moment(date).format('HH:mm:ss');
    }
    static isValid = (value) => {
        return value != null && value != "" && isNaN(value) && !isNaN(Date.parse(value));
    }
}
class Table {
    static add(pram = {
        id: '',
        data: [],
        action: 'destroy',
        overflow: 'auto',
        height: '68vh',
        isHeightPercent: false,
        fontSize: 11,
        filterControl: false,
        showExport: true,
        showfooter: false,
        search: true,
        showPrint: false,
        printTitle: '',
        mobileResponsive: true,
        toggle: true,
        detailFormatter: null,
        thead: [],
        tbody: [],
        autoThead: false,
        actionEvents: false,
        removeExtraColumns: [],
        hideColumns: '',
        reportDesc: '',
        isPrint: false,
        selectPick: false
    }) {
        pram = {
            id: pram.id === undefined ? '' : pram.id,
            data: pram.data == undefined || typeof pram.data != 'object' ? [] : pram.data,
            action: pram.action === undefined ? "destroy" : pram.action,
            overflow: pram.overflow === undefined ? 'auto' : pram.overflow,
            height: pram.height === undefined ? "68vh" : pram.height,
            fontSize: pram.fontSize === undefined ? '11px' : pram.fontSize + 'px',
            filterControl: pram.filterControl === undefined ? false : pram.filterControl,
            search: pram.search === undefined ? true : pram.search,
            showfooter: pram.showfooter === undefined ? true : pram.showfooter,
            printTitle: pram.printTitle == undefined ? 'Print Table' : pram.printTitle,
            mobileResponsive: pram.mobileResponsive === undefined ? true : pram.mobileResponsive,
            detailFormatter: pram.detailFormatter === undefined ? null : pram.detailFormatter,
            thead: pram.thead === undefined ? [] : pram.thead,
            tbody: pram.tbody === undefined ? [] : pram.tbody,
            autoThead: pram.autoThead === undefined ? false : pram.autoThead,
            actionEvent: pram.actionEvent === undefined ? false : pram.actionEvent,
            toggle: pram.toggle === undefined ? false : pram.toggle,
            removeCol: pram.removeCol === undefined ? [] : pram.removeCol,
            reportDesc: pram.reportDesc === undefined ? '' : pram.reportDesc,
            isPrint: pram.isPrint === undefined ? false : pram.isPrint,
            selectPick: pram.selectPick === undefined ? false : pram.selectPick,
            viewMode: pram.viewMode === undefined ? 'Edit' : pram.viewMode
        };
        if (pram.id == '') {
            console.warn('Table id did not find...');
            return;
        }
        if ($(pram.id).attr('data-detail-formatter')) {
            pram.detailFormatter = $(pram.id).attr('data-detail-formatter');
        }

        if (pram.action == 'destroy') {
            $(pram.id).bootstrapTable(pram.action);
            if (pram.thead.length > 0) {
                $(pram.id + ' thead').html(pram.thead.join(''));
            }
            if (pram.autoThead) {
                Table.autoThead(pram);
            }
            if (pram.tbody.length > 0) {
                $(pram.id + ' tbody').html(pram.tbody.join(''));
                $(pram.id).bootstrapTable({ search: pram.search, trimOnSearch: false });
            }
            else {
                $(pram.id).bootstrapTable({
                    data: pram.data,
                    filterControl: pram.filterControl,
                    search: pram.search,
                    showfooter: pram.showfooter,
                    trimOnSearch: false,
                    detailView: pram.detailFormatter != null ? true : false,
                    detailFormatter: pram.detailFormatter,
                    printPageBuilder: (pram) => {
                        return '<html><head><style type="text/css" media="print"> @page { size: auto; margin: 25px 0 25px 0; } </style> <style type="text/css" media="all"> table { border-collapse: collapse; font-size: 12px; } table, th, td { border: 1px solid grey; } th, td { text-align: center; vertical-align: middle; } p { font-weight: bold; margin-left:20px; } table { width:94%; margin-left:3%; margin-right:3%; } div.bs-table-print { text-align:center; } </style> </head><title>' + pram.printTitle + '</title> <body> <p>'.concat(pram.printTitle).concat('<p> <div class="bs-table-print">').concat(pram).concat('</div> </body> </html>')
                    }
                });
            }
        }
        else {
            $(pram.id).bootstrapTable(pram.action, pram.data);
        }
        //Close Drop down menu if scrolled the table body
        //$('div.fixed-table-body').on('scroll', (event) => {
        //    $(event.currentTarget).find('.dropdown-menu.show').removeClass('show');
        //});
        //Table height        
        $(pram.id).parent().css({
            "overflow-y": pram.overflow,
            "overflow-x": pram.overflow,
            "max-height": pram.height,
        });
        //Font size
        $(pram.id).css({ 'font-size': pram.fontSize });
        //Set Responsive
        if (pram.mobileResponsive) {
            $(pram.id).addClass('table-mobile-responsive');
            $(pram.id + ' thead tr:not(.hide) th:not([colspan])').each((index, element) => {
                if ($(element).attr('colspan') === undefined) {
                    var thText = $(element).text().trim();
                    $(pram.id + ' tbody tr td:nth-child(' + (index + 1) + ')').attr('data-title', thText);
                }
            });
        }

        //Set Check As Toggle Botton
        if (pram.toggle) {
            $(`${pram.id} input[type="checkbox"]`).parent('label').parent('div.th-inner').addClass('p-0');
            $(`${pram.id} input[type="checkbox"]`).parent('label').addClass('form-switch ps-0 pt-1');
            $(`${pram.id} input[type="checkbox"]`).addClass('form-check-input');
        }
        if (pram.viewMode == "View" || pram.viewMode == "view") {
            $(`${pram.id} input`).attr('disabled', true);
            $(`${pram.id} textarea`).attr('disabled', true);
            $(`${pram.id} select`).attr('disabled', true);
        }
        //Set Select As Select Pick
        if (pram.selectPick) {
            $(`${pram.id} tbody select.select-pick[data-isMatched="false"]`).val(null);
            Dropdown.refresh({ selector: `${pram.id} tbody select.select-pick` });
        }
        //Print
        if (pram.isPrint) {
            let company = App.User.Company.find(x => x.IsDefault);
            if (company == undefined) {
                console.error('Company Info did not find');
                return;
            }
            let table = document.querySelector(pram.id).cloneNode(true);
            let noOfCol = 0;
            table.tHead.querySelector('tr').querySelectorAll('th').forEach(th => {
                noOfCol += parseInt(th.attributes['colspan'] != undefined ? th.attributes['colspan'].value : 1);
            });
            let title = `<tr>
                            <th colspan="${noOfCol}">
                                <div class="company-name">${company.Description}</div>
                                <div class="company-add">
                                    ${company.Address1 || ``}, ${company.PostOffice || ``}, ${company.District || ``}, ${company.StateName || ``} - ${company.PinCode || ``}
                                </div>
                                <div class="report-desc">${pram.reportDesc}</div>
                            </th>
                        </tr>`;
            $(table.tHead).prepend(title);
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
            Print.table({ title: pram.printTitle, style: style, content: htmlString, orientation: 'A4 landscape', print: false, tableId: '#tableSubGroup' });
        }
        //Note work on search input. for content
    }
    static empty(pram = { selector: '' }) {
        pram = {
            id: pram.selector === undefined || pram.selector == '' ? '.table-default' : pram.selector
        }
        $(pram.id).attr('data-trim-on-search', false);
        $(pram.id).bootstrapTable('destroy');
        $(pram.id).bootstrapTable();
        //Table height        
        $(pram.id).parent().css({
            "overflow-y": 'auto',
            "overflow-x": 'auto',
            "max-height": '68vh'
        });
    }
    static remove(pram = { id: '', value: [-1], field: '$index', toggle: false }) {
        pram.id = typeof (pram.id) == 'undefined' ? '' : pram.id;
        pram.value = typeof (pram.value) == 'undefined' ? [-1] : pram.value;
        pram.field = typeof (pram.field) == 'undefined' ? '$index' : pram.field;
        pram.toggle = pram.toggle === undefined ? false : pram.toggle;
        if (pram.id == '') {
            console.warn('Table Id or trDataIndex is empty.');
            return;
        }
        $(pram.id).bootstrapTable('remove', { values: pram.value, field: pram.field });
        //Set Select As Select Pick
        $(`${pram.id} tbody select.select-pick[data-isMatched="false"]`).val(null);
        Dropdown.refresh({ selector: `${pram.id} tbody select.select-pick` });
        //Set Check As Toggle Botton
        if (pram.toggle) {
            $(`${pram.id} input[type="checkbox"]`).parent('label').parent('div.th-inner').addClass('p-0');
            $(`${pram.id} input[type="checkbox"]`).parent('label').addClass('form-switch ps-0 pt-1');
            $(`${pram.id} input[type="checkbox"]`).addClass('form-check-input');
        }
    }
    static updateById(pram = { id: '', objId: -1, obj: {} }) {
        pram.id = typeof (pram.id) == 'undefined' ? '' : pram.id;
        pram.objId = typeof (pram.objId) == 'undefined' ? -1 : pram.objId;
        pram.obj = typeof (pram.obj) == 'undefined' ? {} : pram.obj;
        if (Field.isNullOrEmpty(pram.id)) {
            console.warn('Table id did not find.');
            return;
        }
        if (pram.objId == -1) {
            console.warn('data id did not find.');
            return;
        }
        if (pram.obj == null) {
            console.warn('value is null.');
            return;
        }
        if ($.isEmptyObject(pram.obj)) {
            console.warn('value is empty.');
            return;
        }
        $(pram.id).bootstrapTable('updateByUniqueId', { id: pram.objId, row: pram.obj });
    }
    static updateByIndex(pram = { id: '', index: -1, obj: {}, value: null, event: null, toggle: false }) {
        pram.id = pram.id === undefined ? '' : pram.id;
        pram.index = pram.index === undefined ? -1 : pram.index;
        pram.obj = pram.obj === undefined ? {} : pram.obj;
        pram.value = pram.value === undefined ? null : pram.value;
        pram.event = pram.event === undefined ? null : pram.event;
        pram.toggle = pram.toggle === undefined ? false : pram.toggle;
        if (Field.isNullOrEmpty(pram.id)) {
            console.warn('Table id did not find.');
            return;
        }
        if (pram.index == -1) {
            console.warn('index no. did not find.');
            return;
        }
        if (pram.obj == null) {
            console.warn('value is null.');
            return;
        }
        if ($.isEmptyObject(pram.obj)) {
            console.warn('value is empty.');
            return;
        }
        //Get Text Cursor Poition
        let curStart = pram.event == null ? -1 : document.getElementById(pram.event.target.id).selectionStart;
        //Update Table By Index
        $(pram.id).bootstrapTable('updateRow', { index: pram.index, row: pram.obj });
        //Set Select As Select Pick
        $(`${pram.id} tbody select.select-pick[data-isMatched="false"]`).val(null);
        Dropdown.refresh({ selector: `${pram.id} tbody select.select-pick` });
        //Set Check As Toggle Botton
        if (pram.toggle) {
            $(`${pram.id} input[type="checkbox"]`).parent('label').parent('div.th-inner').addClass('p-0');
            $(`${pram.id} input[type="checkbox"]`).parent('label').addClass('form-switch ps-0 pt-1');
            $(`${pram.id} input[type="checkbox"]`).addClass('form-check-input');
        }
        //Set Cursor Position and focus
        if (pram.event != null) {
            var field = document.getElementById($(pram.event.target).prop('id'));
            if (pram.event.currentTarget.tagName.toLowerCase() == "select") {
                if (Array.from(pram.event.currentTarget.classList).filter(value => value == 'select-pick').length > 0) {
                    $(`button[data-target="#${field.id}-SelectPick-Menu"]`).trigger('focus');
                }
                else {
                    $(`#${field.id}`).trigger('focus');
                }
            }
            else if (pram.value != null && curStart > -1) {
                var curPos = curStart > 0 && curStart < pram.value.length ? curStart : pram.value.length;
                if (field.type == "date") {
                    $(`#${field.id}`).trigger('focus').val('').val(pram.value);
                }
                else {
                    field.setSelectionRange(curPos, curPos);
                    field.focus();
                }
            }
        }
    }
    static addtree(pram = {
        id: '',
        data: [],
        filterControl: false,
        showFullscreen: false,
        showExport: false,
        search: false,
        mobileResponsive: false,
        showPrint: false,
        idField: '',
        treeShowField: '',
        parentIdField: '',
        treeColumn: 1,
        oprationType: '',
        heightStatus: '',
        height: ''
    }) {
        pram = {
            id: typeof (pram.id) == 'undefined' ? '' : pram.id,
            data: typeof (pram.data) == 'undefined' ? [] : pram.data,
            filterControl: typeof (pram.filterControl) == 'undefined' ? false : pram.filterControl,
            showFullscreen: typeof (pram.showFullscreen) == 'undefined' ? false : pram.showFullscreen,
            showExport: typeof (pram.showExport) == 'undefined' ? false : pram.showExport,
            search: typeof (pram.search) == 'undefined' ? false : pram.search,
            mobileResponsive: typeof (pram.mobileResponsive) == 'undefined' ? false : pram.mobileResponsive,
            showPrint: typeof (pram.showPrint) == 'undefined' ? false : pram.showPrint,
            idField: typeof (pram.idField) == 'undefined' ? '' : pram.idField,
            treeShowField: typeof (pram.treeShowField) == 'undefined' ? '' : pram.treeShowField,
            parentIdField: typeof (pram.parentIdField) == 'undefined' ? '' : pram.parentIdField,
            treeColumn: typeof (pram.treeColumn) == 'undefined' ? 0 : pram.treeColumn,
            oprationType: typeof (pram.oprationType) == 'undefined' ? "destroy" : pram.oprationType,
            heightStatus: typeof (pram.heightStatus) == 'undefined' ? '' : pram.heightStatus,
            height: typeof (pram.height) == 'undefined' ? '400px' : pram.height + 'px',
        }
        if (pram.id == '') {
            console.warn('Table id did not find. So data has not bind in table.');
            return;
        }
        if (pram.data.length == 0 || pram.data == null) {
            $(pram.tableId).bootstrapTable(pram.oprationType);
            $(pram.tableId).bootstrapTable();
            BsTable.tableDesign(pram);
            console.warn('Bs Table Data not found');
            return;
        }
        $(pram.id).bootstrapTable(pram.oprationType);
        $(pram.id).bootstrapTable({
            filterControl: pram.filterControl,
            data: pram.data,
            showFullscreen: pram.showFullscreen,
            showExport: pram.showExport,
            search: pram.search,
            mobileResponsive: pram.mobileResponsive,
            showPrint: pram.showPrint,
            idField: pram.idField,
            treeShowField: pram.treeShowField,
            parentIdField: pram.parentIdField,
            onPostBody: function () {
                $(pram.id).treegrid({
                    treeColumn: pram.treeColumn,
                    expanderExpandedClass: 'fa fa-minus-circle text-primary',
                    expanderCollapsedClass: 'fa fa-plus-circle text-primary',
                    initialState: 'collapsed',
                    onChange: function () {
                        $(pram.id).bootstrapTable('resetView')
                    }
                })
            },
            printPageBuilder: function (pram) {
                return '<html><head><style type="text/css" media="print"> @page { size: auto; margin: 25px 0 25px 0; } </style> <style type="text/css" media="all"> table { border-collapse: collapse; font-size: 12px; } table, th, td { border: 1px solid grey; } th, td { text-align: center; vertical-align: middle; } p { font-weight: bold; margin-left:20px; } table { width:94%; margin-left:3%; margin-right:3%; } div.bs-table-print { text-align:center; } </style> </head><title>' + pram.printTitle + '</title> <body> <p>'.concat(pram.printTitle).concat('<p> <div class="bs-table-print">').concat(pram).concat('</div> </body> </html>')
            }
        });
        $(pram.id).parent().css({
            "overflow-y": "auto",
            "overflow-x": "auto",
            "max-height": pram.isHeightPercent == false ? pram.height : pram.height.replaceAll('px', '%')
        });
        $(pram.id).css({ 'font-size': pram.fontSize });
        if (pram.mobileResponsive) {
            $(pram.id).addClass('table-mobile-responsive');
            $(pram.id + ' thead tr:not(.hide) th').each(function (index) {
                var thText = $(this).text().trim();
                $(pram.id + ' tbody tr td:nth-child(' + (index + 1) + ')').attr('data-title', thText);
            });
        }
    }
    static autoThead(pram = { id: '', data: [], removeCol: [], actionEvent: false }) {
        var thead = [];
        thead.push('<tr>');
        $.map(pram.data[0], (value, fieldName) => {
            var dataField = fieldName;
            var theadText = fieldName.replaceAll('_', ' ').replace(/([A-Z])/g, ' $1').replace(/\s\s+/g, ' ').toCamelCase();
            if (pram.removeCol.indexOf(dataField) == -1) {
                if (DateTime.isValid(value)) {
                    thead.push(`<th data-field="${dataField}" data-formatter="${pram.id.replace("#", "")}Date" class="text-nowrap">${theadText}</th>`);
                }
                else if (fieldName == "Status") {
                    thead.push(`<th data-field="${dataField}" data-formatter="${pram.id.replace("#", "")}Status">${theadText}</th>`);
                }
                else {
                    thead.push(`<th data-field="${dataField}">${theadText}</th>`);
                }
            }
        });
        if (pram.actionEvent == true) {
            thead.push(`
                <th data-formatter="${pram.id.replace('#', '')}Action" data-events="${pram.id.replace('#', '')}Event" class="not-print text-center">
                    <i class="fa-solid fa-list-check"></i>
                </th>
            `);
        }
        thead.push('</tr>');
        $(`${pram.id} thead`).html(thead.join(''));
    }
    static setCursor(pram = { event: {}, value: null, cursorStart: 0, callBack: () => { } }) {
        pram.event = typeof (pram.event) == "undefined" ? null : pram.event;
        pram.value = typeof (pram.value) == "undefined" ? null : pram.value;
        pram.cursorStart = typeof (pram.cursorStart) == "undefined" ? 0 : pram.cursorStart;
        pram.callBack = typeof (pram.callBack) == "undefined" ? () => { } : pram.callBack;

        pram.callBack();
        if (pram.event != null && pram.value != null) {
            var field = document.getElementById($(pram.event.target).prop('id'));
            var cursorPosition = pram.cursorStart > 0 && pram.cursorStart < pram.value.length ? pram.cursorStart : pram.value.length;
            if (field.type == "date") {
                $('#' + field.id).trigger('focus').val('').val(pram.value);
            }
            else {
                field.setSelectionRange(cursorPosition, cursorPosition);
                field.focus();
            }
        }
    }
}
class Dropdown {
    static bind(pram = { id: '', data: [], value: '', text: '', size: 4, icon: '', subText: '', html: '', initialValue: [], json: false, addFn: '', editFn: '', isSelectPick: true, isEditable: false, disabled }) {
        pram.data = typeof (pram.data) == 'undefined' ? [] : pram.data;
        pram.value = typeof (pram.value) == 'undefined' ? 'Id' : pram.value;
        pram.text = typeof (pram.text) == 'undefined' ? 'Text' : pram.text;
        pram.size = typeof (pram.size) == 'undefined' ? 4 : pram.size;
        pram.icon = typeof (pram.icon) == 'undefined' ? '' : pram.icon;
        pram.subText = typeof (pram.subText) == 'undefined' ? '' : pram.subText;
        pram.html = typeof (pram.html) == 'undefined' ? '' : pram.html;
        pram.initialValue = typeof (pram.initialValue) == 'undefined' ? [] : pram.initialValue;
        pram.json = typeof (pram.json) == 'undefined' ? false : pram.json;
        pram.isSelectPick = typeof (pram.isSelectPick) == 'undefined' ? true : pram.isSelectPick;
        pram.isEditable = typeof (pram.isEditable) == 'undefined' ? false : pram.isEditable;

        if (pram.isEditable) {
            $(pram.id).editableSelect('destroy');
        }
        let select = document.querySelector(pram.id);
        if (select == null) {
            console.warn(`Select Id (${pram.id}) is undefined`);
            return;
        }
        if (pram.addFn !== undefined) {
            select.setAttribute('data-action', true);
            select.setAttribute('data-addFn', pram.addFn);
        }
        if (pram.editFn !== undefined) {
            select.setAttribute('data-editFn', pram.editFn);
        }

        select.disabled = pram.disabled === undefined ? select.disabled : pram.disabled;
        select.innerHTML = "";
        const fragment = document.createDocumentFragment();
        let isOptionMatched = false;
        pram.data.forEach((obj, index) => {
            let option = new Option();
            option.value = obj[pram.value];
            option.selected = pram.initialValue.map(value => value == null ? null : value.toString()).includes(option.value);
            isOptionMatched = isOptionMatched ? true : pram.initialValue.map(value => value == null ? null : value.toString()).includes(option.value);
            option.text = obj[pram.text];
            if (obj[pram.icon]) {
                option.setAttribute(SELECTPICK_ITEM_DATA_ICON, obj[pram.icon]);
            }
            if (obj[pram.subText]) {
                option.setAttribute(SELECTPICK_ITEM_DATA_SUBTEXT, obj[pram.subText]);
            }
            if (obj[pram.html]) {
                option.setAttribute(SELECTPICK_ITEM_DATA_HTML, obj[pram.html]);
            }
            if (pram.json) {
                option.setAttribute('data-json', JSON.stringify(obj));
            }
            fragment.appendChild(option);
        });
        select.appendChild(fragment);
        if (!isOptionMatched) {
            select.value = null;
        }
        if (pram.isEditable) {
            $(pram.id).editableSelect({ effects: 'slide' });
        }
        else if (pram.isSelectPick) {
            SelectPick.render(select);
        }
    }
    static bindIcon(pram = { id: '', value: [] }) {
        pram.id = typeof (pram.id) == 'undefined' ? '' : pram.id;
        pram.value = pram.value == undefined ? [] : pram.value
        if (Field.isNullOrEmpty(pram.id)) {
            console.warn('id did not find for bind icon.')
            return;
        }
        Data.get(
            {
                url: "/lib/site/fontawesome.json",
                isApi: false,
                onSuccess: (response) => {
                    Dropdown.bind({ id: pram.id, data: response.data, value: 'Value', text: 'Value', icon: 'Value', initialValue: pram.value });
                }
            }
        );
    }
    static set(pram = { id: '', value: [], text: [] }) {
        if (!pram.id) {
            Message.show({ statusText: 'Id is unidefined' });
            return;
        }
        pram.value = pram.value == undefined ? null : pram.value;
        pram.text = pram.text == undefined ? null : pram.text;
        SelectPick.set({ id: pram.id, value: pram.value, text: pram.text });
    }
    static itemJson(pram = { id: '' }) {
        if ($(`${pram.id} option:selected`).attr('data-json') == undefined) {
            return null;
        }
        else {
            return JSON.parse($(`${pram.id} option:selected`).attr('data-json'));
        }
    }
    static refresh(pram = { selector: '', isDisabled: '' }) {
        if (pram.selector == undefined || Field.isNullOrEmpty(pram.selector)) {
            console.warn('Selector is undefined or empty in the function Dropdown refresh.');
            return;
        }
        if (pram.isDisabled != undefined) {
            $(pram.selector).attr('disabled', pram.isDisabled);
        }
        SelectPick.refresh({ selector: pram.selector });
    }
    static html(pram = { id: '', class: '', data: [], value: '', text: '', size: 4, icon: '', subText: '', html: '', initialValue: [], json: false, title: '', selectionType: '', parent: '', isSelectPick: true, isEditable: false, addFn: '', editFn: '' }) {
        pram.id = typeof (pram.id) == 'undefined' ? '' : pram.id;
        pram.class = typeof (pram.class) == 'undefined' ? '' : pram.class;
        pram.data = typeof (pram.data) == 'undefined' ? [] : pram.data;
        pram.value = typeof (pram.value) == 'undefined' ? 'Id' : pram.value;
        pram.text = typeof (pram.text) == 'undefined' ? 'Text' : pram.text;
        pram.size = typeof (pram.size) == 'undefined' ? 4 : pram.size;
        pram.icon = typeof (pram.icon) == 'undefined' ? '' : pram.icon;
        pram.subText = typeof (pram.subText) == 'undefined' ? '' : pram.subText;
        pram.html = typeof (pram.html) == 'undefined' ? '' : pram.html;
        pram.initialValue = typeof (pram.initialValue) == 'undefined' ? [] : pram.initialValue;
        pram.json = typeof (pram.json) == 'undefined' ? false : pram.json;
        pram.title = typeof (pram.title) == 'undefined' ? 'Nothing Selected' : pram.title;
        pram.selectionType = typeof (pram.selectionType) == 'undefined' ? '' : pram.selectionType;
        pram.parent = typeof (pram.parent) == 'undefined' ? 'body' : pram.parent;
        pram.isSelectPick = typeof (pram.isSelectPick) == 'undefined' ? true : pram.isSelectPick;
        pram.isEditable = typeof (pram.isEditable) == 'undefined' ? false : pram.isEditable;

        let selectHtml = [];
        if (Field.isNullOrEmpty(pram.id)) {
            return selectHtml.join('');
        }
        let isMatched = pram.data.filter(obj => pram.initialValue.toString().includes(obj[pram.value] == null ? "" : obj[pram.value].toString())).length > 0 ? true : false;
        selectHtml.push(`<select id="${pram.id}" class="form-control form-control-sm select-pick ${pram.class}" title="${pram.title}" ${pram.selectionType} data-search="true" data-size="${pram.size}" data-parent="${pram.parent}" data-ismatched="${isMatched}"`);
        if (pram.addFn !== undefined) {
            selectHtml.push(` data-action="true" `);
            selectHtml.push(` data-addFn="${pram.addFn}" `);
        }
        if (pram.editFn !== undefined) {
            selectHtml.push(` data-editFn="${pram.editFn}" `);
        }
        selectHtml.push(`>`);
        pram.data.forEach((obj, index) => {
            let option = '';
            option += `<option `;
            option += `value ='${obj[pram.value].toString().replaceAll("'", "&#39;")}' `;
            option += `${pram.initialValue.map(value => value == null ? null : value.toString()).includes(obj[pram.value].toString()) ? `selected` : ``} `;
            if (obj[pram.icon]) {
                option += `${SELECTPICK_ITEM_DATA_ICON} = '${obj[pram.icon].replaceAll("'", "&#39;")}' `;
            }
            if (obj[pram.subText]) {
                option += `${SELECTPICK_ITEM_DATA_SUBTEXT} = '${obj[pram.subText].replaceAll("'", "&#39;")}' `;
            }
            if (obj[pram.html]) {
                option += `${SELECTPICK_ITEM_DATA_HTML} = '${obj[pram.html].replaceAll("'", "&#39;")}' `;
            }
            if (pram.json) {
                option += `data-json = '${JSON.stringify(obj).replaceAll("'", "&#39;")}' `;
            }
            option += `>`;
            option += `${obj[pram.text]}`;
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