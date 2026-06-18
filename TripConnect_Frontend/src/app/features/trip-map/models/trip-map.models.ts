export interface Coordinate {
  lat: number;
  lng: number;
}

export interface TripDay {
  day: number;
  source: string;
  destination: string;
  stops?: string[];
  color?: string;
  sourceCoord?: Coordinate;
  destCoord?: Coordinate;
  stopsCoords?: Coordinate[];
  routeCoordinates?: Coordinate[];
}

export interface Trip {
  id: string;
  title: string;
  days: TripDay[];
}

export interface AnimationState {
  isPlaying: boolean;
  step: number;
  totalSteps: number;
  progress: number;
}
