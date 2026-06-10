import { create } from 'zustand';

interface MapPickerState {
  pickedLat: number | null;
  pickedLng: number | null;
  pickedAddress: string;
  setPicked: (lat: number, lng: number, address?: string) => void;
  clear: () => void;
}

export const useMapPickerStore = create<MapPickerState>((set) => ({
  pickedLat: null,
  pickedLng: null,
  pickedAddress: '',
  setPicked: (lat, lng, address = '') => set({ pickedLat: lat, pickedLng: lng, pickedAddress: address }),
  clear: () => set({ pickedLat: null, pickedLng: null, pickedAddress: '' }),
}));
