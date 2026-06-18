import { HttpInterceptorFn } from '@angular/common/http';

const EXTERNAL_HOSTS = ['nominatim.openstreetmap.org', 'api.openrouteservice.org'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isExternal = EXTERNAL_HOSTS.some(host => req.url.includes(host));
  if (isExternal) return next(req);

  const token = localStorage.getItem('tc_token');
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  return next(req);
};
