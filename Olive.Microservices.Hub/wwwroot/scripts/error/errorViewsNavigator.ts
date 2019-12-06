/// <amd-dependency path='app/model/service' />
/// <amd-dependency path='app/error/errorTemplates' />
/// <amd-dependency path='app/extensions' />
import Service from 'app/model/service';


export default class ErrorViewsNavigator {
    public static goToServiceError(service: Service, url: string) {
        let errorContent = errorTemplates.SERVICE.replace("[#URL#]", url).replace("[#SERVICE#]", service.Name);
        $("main").replaceWith(errorContent);
        let addressBar = url.trimHttpProtocol().replace(service.BaseUrl.trimHttpProtocol(), service.Name).withPrefix("/");
        window.history.pushState(null, service.Name, addressBar);
    }
}
