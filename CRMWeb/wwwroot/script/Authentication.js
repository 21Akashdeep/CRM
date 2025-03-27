document.addEventListener("DOMContentLoaded", () => {  
    SessionStorage.removeAll();
    Authentication.appInfo();
    Authentication.init();    
});

class Authentication {
    static init() {        
        Util.init();        
        $('#btnLogin').on('click', function () {
            if (!Field.isMandatory({ class: '.required' })) {
                return;
            }
            var obj = {
                UserId: $('#UserId').val(),
                Password: $('#Password').val(),
                ApiType: App.Setting.ApiType.WebApp
            };
            Authentication.login(obj);
        });
    }
    static appInfo() {
        Data.get({
            url: 'Authentication/AppInfo', loader: false, onSuccess: (response) => {                
                App.Setting = response.data;                
                SessionStorage.set('Setting', JSON.stringify(response.data));                
                $('title').text(`${App.Setting.Info.Name} - Login`);
                $('#AppName').text(App.Setting.Info.Desc);
                $('#AppDesc').text(App.Setting.Info.SubDesc);
                $('#LoginInfo').removeClass('hide');
            }
        });
    }
    static login(obj) {        
        Data.post(
            {
                url: 'Authentication/Login',
                data: obj,
                onSuccess: (response) => {
                    if (response.status == Message.Type.success) {
                        let obj = response.data;
                        SessionStorage.set('User', JSON.stringify(obj.User));
                        SessionStorage.set('AppMenu', Data.menuHtmlString(obj.AppMenu));
                        if (obj.AppMenu.length == 0 && obj.User.Type != App.Setting.UserType.SysAdmin) {
                            Message.show({ status: Message.Type.error, statusText: "You are not authorised to access this Application." });
                            return;
                        }
                        location.replace(Url.App + "/Dashboard");

                        if ($(window).width() > 1000) {
                            Cookie.set('SidebarView', "Off");
                        }
                    }
                    else {
                        Message.show(response);
                    }
                }
            },
            
        );
    }
}

