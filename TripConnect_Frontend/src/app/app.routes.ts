import { Routes } from '@angular/router';
import { Register } from './Pages/register/register';
import { Login } from './Pages/login/login';
import { ExporeTrips } from './Pages/expore-trips/expore-trips';
import { Trip } from './Pages/trip/trip';
import { NewTrip } from './Pages/new-trip/new-trip';
import { EditTrip } from './Pages/edit-trip/edit-trip';
import { JoinRequest } from './Pages/join-request/join-request';
import { Profile } from './Pages/profile/profile';
import { ViewProfile } from './Pages/view-profile/view-profile';
import { TripMapComponent } from './features/trip-map/pages/trip-map/trip-map.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  {
    path: '',
    children: [
      {
        path: '',
        redirectTo: 'explore',
        pathMatch: 'full'
      },
      {
        path: 'explore',
        component: ExporeTrips
      },
      {
        path: 'create',
        component: NewTrip,
        canActivate: [authGuard]
      },
      {
        path: 'profile',
        component: Profile,
        canActivate: [authGuard]
      },
      {
        path: 'trip/:id',
        component: Trip,
        canActivate: [authGuard]
      },
      {
        path: 'edit/:id',
        component: EditTrip,
        canActivate: [authGuard]
      },
      {
        path: 'request/:id',
        component: JoinRequest,
        canActivate: [authGuard]
      },
      {
        path: 'user/:id',
        component: ViewProfile,
        canActivate: [authGuard]
      },
      {
        path: 'map',
        component: TripMapComponent,
        canActivate: [authGuard]
      },
      {
        path: 'map/:id',
        component: TripMapComponent,
        canActivate: [authGuard]
      }
    ]
  }
];
