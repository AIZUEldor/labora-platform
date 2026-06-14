import React, { useEffect, useRef, useState } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity,
  ActivityIndicator, TextInput, FlatList, Keyboard,
} from 'react-native';
import { WebView } from 'react-native-webview';
import * as Location from 'expo-location';
import { router, useLocalSearchParams } from 'expo-router';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useThemeStore } from '../store/themeStore';
import { useLanguageStore } from '../stores/useLanguageStore';
import { useMapPickerStore } from '../stores/useMapPickerStore';
import Svg, { Path } from 'react-native-svg';

function BackIcon({ color = '#fff' }: { color?: string }) {
  return (
    <Svg width={22} height={22} viewBox="0 0 24 24" fill="none">
      <Path d="M19 12H5M5 12L12 19M5 12L12 5" stroke={color} strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" />
    </Svg>
  );
}

function SearchIcon({ color = '#999' }: { color?: string }) {
  return (
    <Svg width={18} height={18} viewBox="0 0 24 24" fill="none">
      <Path d="M21 21L16.5 16.5M19 11A8 8 0 1 1 3 11a8 8 0 0 1 16 0z" stroke={color} strokeWidth={1.8} strokeLinecap="round" />
    </Svg>
  );
}

interface SearchResult {
  place_id: string;
  display_name: string;
  lat: string;
  lon: string;
}

export default function MapPickerScreen() {
  const { colors } = useThemeStore();
  const { language } = useLanguageStore();
  const { setPicked } = useMapPickerStore();
  const insets = useSafeAreaInsets();
  const webViewRef = useRef<WebView>(null);
  const params = useLocalSearchParams<{ initLat?: string; initLng?: string }>();

  const [loading, setLoading] = useState(true);
  const [initLat, setInitLat] = useState(41.2995);
  const [initLng, setInitLng] = useState(69.2401);

  const [centerLat, setCenterLat] = useState(41.2995);
  const [centerLng, setCenterLng] = useState(69.2401);
  const [centerAddress, setCenterAddress] = useState('');
  const [addressLoading, setAddressLoading] = useState(false);

  const [searchText, setSearchText] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const [showResults, setShowResults] = useState(false);

  const addressTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const label = (uz: string, ru: string, en: string) =>
    language === 'uz' ? uz : language === 'ru' ? ru : en;

  useEffect(() => {
    const init = async () => {
      if (params.initLat && params.initLng) {
        const lat = parseFloat(params.initLat);
        const lng = parseFloat(params.initLng);
        setInitLat(lat); setInitLng(lng);
        setCenterLat(lat); setCenterLng(lng);
        setLoading(false);
        reverseGeocode(lat, lng);
        return;
      }
      try {
        const { status } = await Location.requestForegroundPermissionsAsync();
        if (status === 'granted') {
          const loc = await Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.Balanced });
          setInitLat(loc.coords.latitude);
          setInitLng(loc.coords.longitude);
          setCenterLat(loc.coords.latitude);
          setCenterLng(loc.coords.longitude);
          reverseGeocode(loc.coords.latitude, loc.coords.longitude);
        }
      } catch {}
      setLoading(false);
    };
    init();
  }, []);

  const reverseGeocode = async (lat: number, lng: number) => {
    setAddressLoading(true);
    try {
      const res = await fetch(
        `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json&accept-language=${language}`,
        { headers: { 'User-Agent': 'ALP-app/1.0' } }
      );
      const data = await res.json();
      if (data.display_name) {
        const parts = data.display_name.split(',').slice(0, 3).join(',');
        setCenterAddress(parts.trim());
      }
    } catch {
      setCenterAddress('');
    } finally {
      setAddressLoading(false);
    }
  };

  const handleSearch = async (text: string) => {
    setSearchText(text);
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    if (text.length < 3) { setSearchResults([]); setShowResults(false); return; }
    searchTimerRef.current = setTimeout(async () => {
      setSearchLoading(true);
      try {
        const res = await fetch(
          `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(text)}&format=json&limit=5&countrycodes=uz&accept-language=${language}`,
          { headers: { 'User-Agent':'ALP-app/1.0'  } }
        );
        const data: SearchResult[] = await res.json();
        setSearchResults(data);
        setShowResults(true);
      } catch {}
      setSearchLoading(false);
    }, 500);
  };

  const handleSelectResult = (item: SearchResult) => {
    const lat = parseFloat(item.lat);
    const lng = parseFloat(item.lon);
    const parts = item.display_name.split(',').slice(0, 3).join(',');
    setCenterLat(lat);
    setCenterLng(lng);
    setCenterAddress(parts.trim());
    setSearchText('');
    setSearchResults([]);
    setShowResults(false);
    Keyboard.dismiss();
    // Xaritani yangi koordinatga ko'chirish
    webViewRef.current?.injectJavaScript(`
      map.setView([${lat}, ${lng}], 16);
      true;
    `);
  };

  const handleWebViewMessage = (event: any) => {
    try {
      const data = JSON.parse(event.nativeEvent.data);
      if (data.type === 'center') {
        const lat = data.lat;
        const lng = data.lng;
        setCenterLat(lat);
        setCenterLng(lng);
        if (addressTimerRef.current) clearTimeout(addressTimerRef.current);
        addressTimerRef.current = setTimeout(() => reverseGeocode(lat, lng), 600);
      }
    } catch {}
  };

  const handleConfirm = () => {
    setPicked(centerLat, centerLng, centerAddress);
    router.back();
  };

  const getMapHtml = (lat: number, lng: number): string => {
    return `<!DOCTYPE html>
<html>
<head>
  <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0">
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    html, body, #map { width: 100%; height: 100%; overflow: hidden; }
    #pin {
      position: absolute;
      top: 50%; left: 50%;
      transform: translate(-50%, -100%);
      z-index: 1000;
      pointer-events: none;
      display: flex;
      flex-direction: column;
      align-items: center;
    }
    #pin .dot {
      width: 22px; height: 22px;
      background: #16A34A;
      border: 3px solid #fff;
      border-radius: 50%;
      box-shadow: 0 3px 8px rgba(0,0,0,0.4);
    }
    #pin .tail {
      width: 0; height: 0;
      border-left: 7px solid transparent;
      border-right: 7px solid transparent;
      border-top: 10px solid #16A34A;
      margin-top: -2px;
    }
    #shadow {
      position: absolute;
      top: 50%; left: 50%;
      transform: translate(-50%, 2px);
      width: 12px; height: 4px;
      background: rgba(0,0,0,0.2);
      border-radius: 50%;
      z-index: 999;
      pointer-events: none;
    }
  </style>
</head>
<body>
  <div id="map"></div>
  <div id="pin"><div class="dot"></div><div class="tail"></div></div>
  <div id="shadow"></div>
  <script>
    var map = L.map('map', { zoomControl: true, attributionControl: false }).setView([${lat}, ${lng}], 16);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19 }).addTo(map);

    // Xarita to'xtaganda markazni yuborish
    map.on('moveend', function() {
      var c = map.getCenter();
      window.ReactNativeWebView.postMessage(JSON.stringify({ type: 'center', lat: c.lat, lng: c.lng }));
    });

    // Pin animate
    var pin = document.getElementById('pin');
    map.on('movestart', function() { pin.style.transform = 'translate(-50%, -110%)'; });
    map.on('moveend',   function() { pin.style.transform = 'translate(-50%, -100%)'; });
    pin.style.transition = 'transform 0.15s ease';
  </script>
</body>
</html>`;
  };

  if (loading) {
    return (
      <View style={[styles.centered, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color="#16A34A" />
      </View>
    );
  }

  return (
    <View style={{ flex: 1 }}>
      {/* Header */}
      <LinearGradient
        colors={['#16A34A', '#15803D']}
        style={[styles.header, { paddingTop: insets.top + 12 }]}
      >
        <TouchableOpacity onPress={() => router.back()} activeOpacity={0.7}>
          <BackIcon />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>
          {label('Joylashuvni belgilang', 'Укажите местоположение', 'Mark Location')}
        </Text>
        <View style={{ width: 22 }} />
      </LinearGradient>

      {/* Qidiruv */}
      <View style={[styles.searchWrapper, { backgroundColor: colors.background }]}>
        <View style={[styles.searchBox, { backgroundColor: colors.card, borderColor: colors.border }]}>
          <SearchIcon color={colors.textTertiary} />
          <TextInput
            style={[styles.searchInput, { color: colors.textPrimary }]}
            placeholder={label('Manzil qidiring...', 'Поиск адреса...', 'Search address...')}
            placeholderTextColor={colors.textTertiary}
            value={searchText}
            onChangeText={handleSearch}
            returnKeyType="search"
          />
          {searchLoading && <ActivityIndicator size="small" color="#16A34A" />}
        </View>

        {showResults && searchResults.length > 0 && (
          <View style={[styles.resultsList, { backgroundColor: colors.card, borderColor: colors.border }]}>
            <FlatList
              data={searchResults}
              keyExtractor={(item) => item.place_id}
              keyboardShouldPersistTaps="always"
              renderItem={({ item }) => (
                <TouchableOpacity
                  style={[styles.resultItem, { borderBottomColor: colors.border }]}
                  onPress={() => handleSelectResult(item)}
                  activeOpacity={0.7}
                >
                  <Text style={[styles.resultText, { color: colors.textPrimary }]} numberOfLines={2}>
                    {item.display_name}
                  </Text>
                </TouchableOpacity>
              )}
            />
          </View>
        )}
      </View>

      {/* Xarita */}
      <View style={{ flex: 1 }}>
        <WebView
          ref={webViewRef}
          source={{ html: getMapHtml(initLat, initLng) }}
          style={{ flex: 1 }}
          onMessage={handleWebViewMessage}
          javaScriptEnabled
          domStorageEnabled
          originWhitelist={['*']}
        />
      </View>

      {/* Pastki panel — manzil + tasdiqlash */}
      <View style={[styles.bottomPanel, { backgroundColor: colors.background, borderTopColor: colors.border, paddingBottom: insets.bottom + 12 }]}>
        <View style={styles.addressRow}>
          <Svg width={16} height={16} viewBox="0 0 24 24" fill="none">
            <Path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7z" stroke="#16A34A" strokeWidth={1.8} />
            <Path d="M12 11.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z" stroke="#16A34A" strokeWidth={1.8} />
          </Svg>
          <Text style={[styles.addressText, { color: colors.textPrimary }]} numberOfLines={2}>
            {addressLoading
              ? label('Manzil aniqlanmoqda...', 'Определение адреса...', 'Getting address...')
              : centerAddress || `${centerLat.toFixed(5)}, ${centerLng.toFixed(5)}`}
          </Text>
        </View>

        <TouchableOpacity style={styles.confirmBtn} onPress={handleConfirm} activeOpacity={0.85}>
          <LinearGradient colors={['#16A34A', '#15803D']} style={styles.confirmGradient}>
            <Text style={styles.confirmText}>
              {label('Shu joyni tasdiqlash', 'Подтвердить место', 'Confirm Location')}
            </Text>
          </LinearGradient>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  centered: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: 20, paddingBottom: 16 },
  headerTitle: { fontSize: 16, fontWeight: '700', color: '#fff' },
  searchWrapper: { paddingHorizontal: 12, paddingVertical: 10, zIndex: 100 },
  searchBox: { flexDirection: 'row', alignItems: 'center', borderRadius: 12, borderWidth: 1.5, paddingHorizontal: 12, paddingVertical: 10, gap: 8 },
  searchInput: { flex: 1, fontSize: 14 },
  resultsList: { marginTop: 4, borderRadius: 12, borderWidth: 1.5, maxHeight: 200, overflow: 'hidden' },
  resultItem: { paddingHorizontal: 14, paddingVertical: 12, borderBottomWidth: StyleSheet.hairlineWidth },
  resultText: { fontSize: 13 },
  bottomPanel: { borderTopWidth: 1, paddingHorizontal: 16, paddingTop: 14, gap: 12 },
  addressRow: { flexDirection: 'row', alignItems: 'flex-start', gap: 8 },
  addressText: { flex: 1, fontSize: 14, fontWeight: '500' },
  confirmBtn: { borderRadius: 14, overflow: 'hidden' },
  confirmGradient: { paddingVertical: 15, alignItems: 'center' },
  confirmText: { color: '#fff', fontSize: 15, fontWeight: '700' },
});
