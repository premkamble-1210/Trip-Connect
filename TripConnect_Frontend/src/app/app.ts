import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Login } from './Pages/login/login';
import { Register } from './Pages/register/register';
import { Navbar } from './Components/navbar/navbar';
import { ExporeTrips } from './Pages/expore-trips/expore-trips';
import { Trip } from './Pages/trip/trip';
import { NewTrip } from './Pages/new-trip/new-trip';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet,Register,Login,Navbar,ExporeTrips,Trip,NewTrip],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('TripConnect_Frontend');
}
