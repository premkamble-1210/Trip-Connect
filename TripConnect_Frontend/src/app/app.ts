import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Login } from './Pages/login/login';
import { Register } from './Pages/register/register';
import { Navbar } from './Components/navbar/navbar';
import { ExporeTrips } from './Pages/expore-trips/expore-trips';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet,Register,Login,Navbar,ExporeTrips],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('TripConnect_Frontend');
}
