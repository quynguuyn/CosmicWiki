export interface Star {
  id: string;
  name: string | null;
  objectTypes: StarObjectType[];
  catalogIdentifiers: StarCatalogIdentifier[];
  coordinates: StarCoordinate[];
  properMotions: StarProperMotion[];
  parallaxes: StarParallax[];
  radialVelocities: StarRadialVelocity[];
  distances: StarDistance[];
  spectralClassifications: StarSpectralClassification[];
  photometricMeasurements: StarPhotometricMeasurement[];
  atmosphericParameters: StarAtmosphericParameters[];
  diameters: StarDiameter[];
  projectedRotationalVelocities: StarProjectedRotationalVelocity[];
  variableStarMeasurements: StarVariableMeasurement[];
}

export interface StarObjectType {
  code: string;
  name: string;
  description: string | null;
}

export interface StarCatalogIdentifier {
  catalog: string;
  value: string;
}

export interface StarCoordinate {
  rightAscension: number;
  declination: number;
  coordinateSystem: number;
  epoch: number;
  wavelengthBand: number | null;
  uncertainty: { majorAxis: number; minorAxis: number; positionAngle: number } | null;
  quality: number;
  referenceId: string | null;
}

export interface StarProperMotion {
  rightAscension: number;
  declination: number;
  uncertainty: { majorAxis: number; minorAxis: number; positionAngle: number } | null;
  quality: number;
  referenceId: string | null;
}

export interface StarParallax {
  valueMas: number;
  uncertainty: { negative: number | null; positive: number | null } | null;
  quality: number;
  referenceId: string | null;
}

export interface StarRadialVelocity {
  valueKmPerSecond: number;
  uncertainty: { negative: number | null; positive: number | null } | null;
  wavelengthBand: number | null;
  quality: number;
  referenceId: string | null;
}

export interface StarDistance {
  value: number;
  unit: string;
  uncertainty: { negative: number | null; positive: number | null } | null;
  method: number;
  quality: number;
  referenceId: string | null;
}

export interface StarSpectralClassification {
  rawType: string;
  temperatureClass: number | null;
  subtype: string | null;
  luminosityClass: string | null;
  peculiarities: string[];
  quality: number;
  referenceId: string | null;
}

export interface StarPhotometricMeasurement {
  filter: number;
  system: number;
  valueType: number;
  measurement: {
    value: number;
    unit: string;
    uncertainty: { negative: number | null; positive: number | null } | null;
    quality: number;
    referenceId: string | null;
  };
  flags: { isJoined: boolean; variabilityCode: string | null } | null;
}

export interface StarAtmosphericParameters {
  effectiveTemperature: { kelvin: number } | null;
  surfaceGravity: { value: number } | null;
  metallicity: { feH: number } | null;
  metallicityFlags: number;
  comparisonStar: string | null;
  catalogNumber: string | null;
  referenceId: string | null;
}

export interface StarDiameter {
  value: number;
  unit: string;
  uncertainty: { negative: number | null; positive: number | null } | null;
  filterOrWavelength: string | null;
  method: string | null;
  quality: number;
  referenceId: string | null;
}

export interface StarProjectedRotationalVelocity {
  value: number;
  uncertainty: { negative: number | null; positive: number | null } | null;
  measurementCount: number | null;
  quality: number;
  referenceId: string | null;
}

export interface StarVariableMeasurement {
  variabilityType: string;
  maximumBrightness: number | null;
  minimumBrightness: number | null;
  photometricBand: string | null;
  periodDays: number | null;
  epochJulianDay: number | null;
  characteristicPercent: number | null;
  referenceId: string | null;
}

export interface AstronomicalImage {
  id: string;
  astronomicalObjectId: string;
  url: string;
  urls: string[];
}

export interface AstronomicalObject {
  id: string;
  name: string | null;
  kind: string;
  objectType: string | null;
  catalog: string | null;
  catalogValue: string | null;
  rightAscensionDegrees: number | null;
  declinationDegrees: number | null;
  imageUrl: string | null;
}
