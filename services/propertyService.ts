import api from './api';
import { CreatePropertyRequest, PropertyListing, PropertyMarker, PropertyType } from '../types';

export const propertyService = {
  getProperties: async (): Promise<PropertyListing[]> => {
    const response = await api.get<PropertyListing[]>('/PropertyListing');
    return response.data;
  },

  getPropertyById: async (id: string): Promise<PropertyListing> => {
    const response = await api.get<PropertyListing>(`/PropertyListing/${id}`);
    return response.data;
  },

  getMyProperties: async (): Promise<PropertyListing[]> => {
    const response = await api.get<PropertyListing[]>('/PropertyListing/my');
    return response.data;
  },

  getPropertyMarkers: async (): Promise<PropertyMarker[]> => {
    const response = await api.get<PropertyMarker[]>('/PropertyListing/markers');
    return response.data;
  },

  // Single combined multipart request: the listing and its 1-10 images are created together in
  // one POST (unlike Job/WorkerPost, which create the entity first and upload images afterward).
  // Field names must match PropertyListingCreateForm's C# property names exactly, and every image
  // is appended under the same 'Images' field name so ASP.NET Core binds them into one List<IFormFile>.
  createProperty: async (data: CreatePropertyRequest, imageUris: string[]): Promise<PropertyListing> => {
    const formData = new FormData();
    formData.append('Title', data.title);
    formData.append('Description', data.description);
    formData.append('PropertyType', String(data.propertyType));
    formData.append('RoomCount', String(data.roomCount));
    formData.append('AreaSquareMeters', String(data.areaSquareMeters));

    // FloorNumber/TotalFloors must never be sent for House - the backend validator requires them
    // to be null for House and rejects the request otherwise.
    if (data.propertyType === PropertyType.Apartment) {
      if (data.floorNumber != null) formData.append('FloorNumber', String(data.floorNumber));
      if (data.totalFloors != null) formData.append('TotalFloors', String(data.totalFloors));
    }

    formData.append('RenovationStatus', String(data.renovationStatus));
    formData.append('Price', String(data.price));
    formData.append('RentalPeriod', String(data.rentalPeriod));
    formData.append('Address', data.address);
    formData.append('Latitude', String(data.latitude));
    formData.append('Longitude', String(data.longitude));
    formData.append('ContactPhoneNumber', data.contactPhoneNumber);

    imageUris.forEach((uri, index) => {
      formData.append('Images', {
        uri,
        name: `property_${index}.jpg`,
        type: 'image/jpeg',
      } as any);
    });

    const response = await api.post<PropertyListing>('/PropertyListing', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  // JSON body (not multipart) - the PUT endpoint never touches images, so there is nothing to
  // upload here. ASP.NET Core's default [FromBody] binding is case-insensitive, so the camelCase
  // CreatePropertyRequest shape can be sent directly, matching jobService.createJob's convention.
  updateProperty: async (id: string, data: CreatePropertyRequest): Promise<PropertyListing> => {
    const response = await api.put<PropertyListing>(`/PropertyListing/${id}`, data);
    return response.data;
  },
};
