import { Routes } from '@angular/router';
import { Register } from './Pages/register/register';
import { Login } from './Pages/login/login';
import { ExporeTrips } from './Pages/expore-trips/expore-trips';
import { Trip } from './Pages/trip/trip';
import { NewTrip } from './Pages/new-trip/new-trip';
import { JoinRequest } from './Pages/join-request/join-request';
import { Profile } from './Pages/profile/profile';

export const routes: Routes = [
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
        component: NewTrip
      },
      {
        path: 'profile',
        component: Profile
      },
      {
        path: 'trip/:id',
        component: Trip
      }
    ]
  }
];
