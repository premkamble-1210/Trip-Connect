import { Injectable } from '@angular/core';
import * as L from 'leaflet';
import { Coordinate } from '../models/trip-map.models';

const OSM_TILE = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
const OSM_ATTRIBUTION = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors';

const CAR_SVG = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48" width="36" height="36">
  <circle cx="24" cy="24" r="22" fill="#0e7c7b" opacity="0.15"/>
  <circle cx="24" cy="24" r="16" fill="#0e7c7b"/>
  <text x="24" y="30" text-anchor="middle" font-size="18" fill="white">🚗</text>
</svg>`;

@Injectable({ providedIn: 'root' })
export class MapService {
  private map!: L.Map;
  private routeLayers: L.Layer[] = [];
  private vehicleMarker: L.Marker | null = null;

  initMap(elementId: string, center: [number, number] = [17.5, 73.9], zoom = 7): L.Map {
    if (this.map) {
      this.map.remove();
    }

    this.map = L.map(elementId, { zoomControl: false }).setView(center, zoom);

    L.tileLayer(OSM_TILE, {
      attribution: OSM_ATTRIBUTION,
      maxZoom: 19,
    }).addTo(this.map);

    L.control.zoom({ position: 'bottomright' }).addTo(this.map);

    return this.map;
  }

  getMap(): L.Map {
    return this.map;
  }

  addStartMarker(coord: Coordinate, label: string): L.Marker {
    const icon = L.divIcon({
      className: '',
      html: `<div style="background:#22c55e;color:#fff;border-radius:50%;width:32px;height:32px;display:flex;align-items:center;justify-content:center;font-size:13px;font-weight:700;box-shadow:0 2px 6px rgba(0,0,0,0.3);border:2px solid #fff">S</div>`,
      iconSize: [32, 32],
      iconAnchor: [16, 16],
      popupAnchor: [0, -20],
    });
    const marker = L.marker([coord.lat, coord.lng], { icon })
      .bindPopup(`<b>Start:</b> ${label}`)
      .addTo(this.map);
    this.routeLayers.push(marker);
    return marker;
  }

  addEndMarker(coord: Coordinate, label: string): L.Marker {
    const icon = L.divIcon({
      className: '',
      html: `<div style="background:#ef4444;color:#fff;border-radius:50%;width:32px;height:32px;display:flex;align-items:center;justify-content:center;font-size:13px;font-weight:700;box-shadow:0 2px 6px rgba(0,0,0,0.3);border:2px solid #fff">E</div>`,
      iconSize: [32, 32],
      iconAnchor: [16, 16],
      popupAnchor: [0, -20],
    });
    const marker = L.marker([coord.lat, coord.lng], { icon })
      .bindPopup(`<b>End:</b> ${label}`)
      .addTo(this.map);
    this.routeLayers.push(marker);
    return marker;
  }

  addDayWaypointMarker(coord: Coordinate, dayNumber: number, label: string): L.Marker {
    const icon = L.divIcon({
      className: '',
      html: `<div style="background:#0e7c7b;color:#fff;border-radius:50%;width:30px;height:30px;display:flex;align-items:center;justify-content:center;font-size:12px;font-weight:700;box-shadow:0 2px 6px rgba(0,0,0,0.3);border:2px solid #fff">${dayNumber}</div>`,
      iconSize: [30, 30],
      iconAnchor: [15, 15],
      popupAnchor: [0, -18],
    });
    const marker = L.marker([coord.lat, coord.lng], { icon })
      .bindPopup(`<b>Day ${dayNumber}:</b> ${label}`)
      .addTo(this.map);
    this.routeLayers.push(marker);
    return marker;
  }

  addStopMarker(coord: Coordinate, label: string): L.Marker {
    const icon = L.divIcon({
      className: '',
      html: `<div style="background:#f59e0b;color:#fff;border-radius:50%;width:26px;height:26px;display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:700;box-shadow:0 2px 4px rgba(0,0,0,0.25);border:2px solid #fff">●</div>`,
      iconSize: [26, 26],
      iconAnchor: [13, 13],
      popupAnchor: [0, -16],
    });
    const marker = L.marker([coord.lat, coord.lng], { icon })
      .bindPopup(`<b>Stop:</b> ${label}`)
      .addTo(this.map);
    this.routeLayers.push(marker);
    return marker;
  }

  drawPolyline(coords: Coordinate[], color: string): L.Polyline {
    const latLngs = coords.map(c => [c.lat, c.lng] as [number, number]);
    const line = L.polyline(latLngs, {
      color,
      weight: 5,
      opacity: 0.85,
      lineJoin: 'round',
      lineCap: 'round',
      dashArray: '10, 8',
    }).addTo(this.map);
    this.routeLayers.push(line);
    return line;
  }

  createVehicleMarker(coord: Coordinate): L.Marker {
    if (this.vehicleMarker) {
      this.vehicleMarker.remove();
    }
    const icon = L.divIcon({
      className: '',
      html: CAR_SVG,
      iconSize: [36, 36],
      iconAnchor: [18, 18],
    });
    this.vehicleMarker = L.marker([coord.lat, coord.lng], { icon, zIndexOffset: 1000 }).addTo(this.map);
    return this.vehicleMarker;
  }

  moveVehicleMarker(coord: Coordinate): void {
    this.vehicleMarker?.setLatLng([coord.lat, coord.lng]);
  }

  removeVehicleMarker(): void {
    this.vehicleMarker?.remove();
    this.vehicleMarker = null;
  }

  clearRouteLayers(): void {
    this.routeLayers.forEach(l => l.remove());
    this.routeLayers = [];
    this.removeVehicleMarker();
  }

  fitBounds(coords: Coordinate[]): void {
    if (!coords.length) return;
    const bounds = L.latLngBounds(coords.map(c => [c.lat, c.lng] as [number, number]));
    this.map.fitBounds(bounds, { padding: [60, 60] });
  }
}
