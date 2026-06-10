import React, { useEffect, useRef, useState } from 'react';
import {
  View, Text, StyleSheet, ActivityIndicator,
  TouchableOpacity, ScrollView, TextInput, FlatList, Keyboard,
} from 'react-native';
import { WebView } from 'react-native-webview';
import * as Location from 'expo-location';
import { useThemeStore } from '../../store/themeStore';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { jobService } from '../../services/jobService';
import { NearbyJob } from '../../types';
import { useLanguageStore } from '../../stores/useLanguageStore';
import Svg, { Path } from 'react-native-svg';

function SearchIcon({ color = '#999' }: { color?: string }) {
  return (
    <Svg width={16} height={16} viewBox="0 0 24 24" fill="none">
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

export default function MapScreen() {
  const { colors } = useThemeStore();
  const { t, language } = useLanguageStore();
  const insets = useSafeAreaInsets();
  const webViewRef = useRef<WebView>(null);

  const [loading, setLoading] = useState(true);
  const [locationError, setLocationError] = useState(false);
  const [userLocation, setUserLocation] = useState<{ lat: number; lng: number } | null>(null);
  const [jobs, setJobs] = useState<NearbyJob[]>([]);
  const [selectedType, setSelectedType] = useState(0);
  const [mapReady, setMapReady] = useState(false);

  const [searchText, setSearchText] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const [showResults, setShowResults] = useState(false);
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const JOB_TYPE_FILTERS = [
    { label: t.map.all,      value: 0 },
    { label: t.map.daily,    value: 1 },
    { label: t.map.seasonal, value: 2 },
    { label: t.map.monthly,  value: 3 },
    { label: t.map.partTime, value: 4 },
    { label: t.map.fullTime, value: 5 },
    { label: t.map.remote,   value: 6 },
  ];

  useEffect(() => { initMap(); }, []);

  useEffect(() => {
    if (!mapReady) return;
    const filtered = selectedType === 0 ? jobs : jobs.filter(j => j.jobType === selectedType);
    webViewRef.current?.injectJavaScript(buildUpdateMarkersJs(filtered));
  }, [selectedType, mapReady]);

  const initMap = async () => {
    setLoading(true);
    setLocationError(false);
    setMapReady(false);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') { setLocationError(true); setLoading(false); return; }
      const location = await Location.getCurrentPositionAsync({ accuracy: Location.Accuracy.Balanced });
      const lat = location.coords.latitude;
      const lng = location.coords.longitude;
      setUserLocation({ lat, lng });
      const nearbyJobs = await jobService.getAllActiveJobs(lat, lng);
      setJobs(nearbyJobs);
    } catch {
      setLocationError(true);
    } finally {
      setLoading(false);
    }
  };

  const loadJobsForLocation = async (lat: number, lng: number) => {
    try {
      const nearbyJobs = await jobService.getAllActiveJobs(lat, lng);
      setJobs(nearbyJobs);
      // Markerlarni yangilash
      console.log('JOBS COUNT:', jobs.length);
      const filtered = selectedType === 0 ? nearbyJobs : nearbyJobs.filter(j => j.jobType === selectedType);
      webViewRef.current?.injectJavaScript(buildUpdateMarkersJs(filtered));
    } catch {}
  };

  const handleSearch = async (text: string) => {
    setSearchText(text);
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    if (text.length < 3) { setSearchResults([]); setShowResults(false); return; }
    searchTimerRef.current = setTimeout(async () => {
      setSearchLoading(true);
      try {
        const res = await fetch(
          `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(text)}&format=json&limit=5&accept-language=${language}`,
          { headers: { 'User-Agent': 'TOP-app/1.0' } }
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
    setSearchText('');
    setSearchResults([]);
    setShowResults(false);
    Keyboard.dismiss();
    webViewRef.current?.injectJavaScript(`
      map.setView([${lat}, ${lng}], 13);
      true;
    `);
    loadJobsForLocation(lat, lng);
  };

  const filteredJobs = selectedType === 0 ? jobs : jobs.filter(j => j.jobType === selectedType);

  const buildMarkersJs = (jobs: NearbyJob[]): string => {
    return jobs.map(job => {
      const title = JSON.stringify(job.title);
      const city = JSON.stringify(job.city ?? '');
      const id = JSON.stringify(job.id);
      const salary = job.salary.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ') + " so'm";
      const salaryStr = JSON.stringify(salary);
      const viewJob = JSON.stringify(t.map.viewJob);
      const away = JSON.stringify(t.map.away);
      const dist = job.distanceKm.toFixed(1);

      return `
        (function() {
          var title = ${title};
          var city = ${city};
          var id = ${id};
          var salary = ${salaryStr};
          var viewJob = ${viewJob};
          var away = ${away};

          function makeIcon(zoom) {
            if (zoom >= 13) {
              var html = '<div style="display:flex;flex-direction:column;align-items:center;transform:translateX(-50%);">'
                + '<div style="background:#16A34A;color:#fff;font-size:12px;font-weight:700;padding:5px 11px;border-radius:20px;border:2px solid #fff;box-shadow:0 3px 8px rgba(0,0,0,0.35);white-space:nowrap;">' + salary + '</div>'
                + '<div style="width:0;height:0;border-left:6px solid transparent;border-right:6px solid transparent;border-top:8px solid #16A34A;margin-top:-1px;"></div>'
                + '</div>';
              return L.divIcon({ html: html, iconAnchor: [0, 38], className: '' });
            }
            var dot = '<div style="width:14px;height:14px;background:#16A34A;border:2.5px solid #fff;border-radius:50%;box-shadow:0 2px 6px rgba(0,0,0,0.4);"></div>';
            return L.divIcon({ html: dot, iconAnchor: [7, 7], className: '' });
          }

          var m = L.marker([${job.latitude}, ${job.longitude}], { icon: makeIcon(map.getZoom()) }).addTo(map);
          map.on('zoomend', function() { m.setIcon(makeIcon(map.getZoom())); });
          m.bindPopup(
  '<div style="min-width:190px;font-family:sans-serif;padding:6px;">'
  + '<b style="font-size:14px;color:#111;display:block;margin-bottom:4px;">' + title + '</b>'
  + '<span style="color:#16A34A;font-weight:700;font-size:14px;">' + salary + '</span><br/>'
  + '<span style="color:#555;font-size:12px;">' + city + '</span><br/>'
  + '<span style="color:#999;font-size:11px;">${dist} ' + away + '</span><br/>'
  + '<button class="view-job-btn" data-id="' + id.replace(/"/g, '') + '" '
  + 'style="margin-top:10px;background:#16A34A;color:#fff;border:none;padding:8px 14px;border-radius:8px;cursor:pointer;font-size:13px;width:100%;font-weight:600;">' + viewJob + '</button>'
  + '</div>'
);
          jobMarkers.push(m);
        })();
      `;
    }).join('\n');
  };

  const buildUpdateMarkersJs = (jobs: NearbyJob[]): string => {
    return `
      (function() {
        jobMarkers.forEach(function(m) { map.removeLayer(m); });
        jobMarkers = [];
        ${buildMarkersJs(jobs)}
      })();
      true;
    `;
  };

  const getMapHtml = (lat: number, lng: number, jobs: NearbyJob[]): string => {
    const youAreHere = JSON.stringify(t.map.youAreHere);
    const markersJs = buildMarkersJs(jobs);

    return `<!DOCTYPE html>
<html>
<head>
  <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0">
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    html, body, #map { width: 100%; height: 100%; overflow: hidden; }
  </style>
</head>
<body>
  <div id="map"></div>
  <script>
    var jobMarkers = [];
    var map = L.map('map', { zoomControl: true }).setView([${lat}, ${lng}], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap', maxZoom: 19,
    }).addTo(map);

    var meHtml = '<div style="display:flex;flex-direction:column;align-items:center;transform:translateX(-50%);">'
      + '<div style="background:#16A34A;color:#fff;font-size:12px;font-weight:700;padding:5px 12px;border-radius:20px;border:2px solid #fff;box-shadow:0 3px 8px rgba(0,0,0,0.4);">me</div>'
      + '<div style="width:0;height:0;border-left:6px solid transparent;border-right:6px solid transparent;border-top:8px solid #16A34A;margin-top:-1px;"></div>'
      + '</div>';
    L.marker([${lat}, ${lng}], {
      icon: L.divIcon({ html: meHtml, iconAnchor: [0, 38], className: '' })
    }).addTo(map).bindPopup('<b>' + ${youAreHere} + '</b>');

    ${markersJs}

    document.addEventListener('click', function(e) {
  var btn = e.target.closest('.view-job-btn');

  if (btn) {
    var jobId = btn.getAttribute('data-id');
    window.ReactNativeWebView.postMessage(jobId);
  }
});

    map.whenReady(function() {
      window.ReactNativeWebView.postMessage('MAP_READY');
    });
  </script>
</body>
</html>`;
  };

  const handleWebViewMessage = (event: any) => {
    console.log('MAP MESSAGE:', event.nativeEvent.data);
  const raw = event.nativeEvent.data;
  if (raw === 'MAP_READY') { setMapReady(true); return; }
  try {
    const id = JSON.parse(raw);
    if (id) router.push({ pathname: '/job-detail', params: { id: String(id) } });
  } catch {
    if (raw) router.push({ pathname: '/job-detail', params: { id: raw } });
  }
};

  if (loading) {
    return (
      <View style={[styles.centered, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color="#16A34A" />
        <Text style={[styles.loadingText, { color: colors.textSecondary }]}>{t.map.locating}</Text>
      </View>
    );
  }

  if (locationError) {
    return (
      <View style={[styles.centered, { backgroundColor: colors.background }]}>
        <Text style={[styles.errorText, { color: colors.textPrimary }]}>{t.map.permissionDenied}</Text>
        <TouchableOpacity style={styles.retryBtn} onPress={initMap}>
          <Text style={styles.retryText}>{t.map.retry}</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={{ flex: 1, backgroundColor: colors.background }}>
      <LinearGradient
        colors={['#16A34A', '#15803D']}
        style={[styles.header, { paddingTop: insets.top + 12 }]}
      >
        <Text style={styles.headerTitle}>{t.map.title}</Text>
        <Text style={styles.headerSub}>{filteredJobs.length} {t.map.found}</Text>
      </LinearGradient>

      {/* Qidiruv */}
      <View style={[styles.searchWrapper, { backgroundColor: colors.background }]}>
        <View style={[styles.searchBox, { backgroundColor: colors.card, borderColor: colors.border }]}>
          <SearchIcon color={colors.textTertiary} />
          <TextInput
            style={[styles.searchInput, { color: colors.textPrimary }]}
            placeholder={t.map.searchPlaceholder ?? 'Shahar, tuman qidiring...'}
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

      {/* Filter chips */}
      <View style={[styles.filterWrapper, { backgroundColor: colors.background }]}>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.filterScroll}>
          {JOB_TYPE_FILTERS.map(f => {
            const active = selectedType === f.value;
            return (
              <TouchableOpacity
                key={f.value}
                onPress={() => setSelectedType(f.value)}
                style={[
                  styles.chip,
                  active
                    ? { backgroundColor: '#16A34A', borderColor: '#16A34A' }
                    : { backgroundColor: colors.card, borderColor: colors.border },
                ]}
              >
                <Text style={[styles.chipText, { color: active ? '#fff' : colors.textPrimary }]}>
                  {f.label}
                </Text>
              </TouchableOpacity>
            );
          })}
        </ScrollView>
      </View>

      {userLocation && (
        <WebView
          ref={webViewRef}
          source={{ html: getMapHtml(userLocation.lat, userLocation.lng, jobs) }}
          style={{ flex: 1 }}
          onMessage={handleWebViewMessage}
          javaScriptEnabled
          domStorageEnabled
          originWhitelist={['*']}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  centered: { flex: 1, alignItems: 'center', justifyContent: 'center', gap: 12 },
  loadingText: { fontSize: 14, marginTop: 8 },
  errorText: { fontSize: 16, fontWeight: '600', textAlign: 'center', paddingHorizontal: 32 },
  retryBtn: { backgroundColor: '#16A34A', paddingHorizontal: 24, paddingVertical: 12, borderRadius: 10 },
  retryText: { color: '#fff', fontWeight: '600', fontSize: 15 },
  header: { paddingHorizontal: 16, paddingBottom: 16, alignItems: 'center' },
  headerTitle: { fontSize: 18, fontWeight: '700', color: '#fff' },
  headerSub: { fontSize: 13, color: 'rgba(255,255,255,0.8)', marginTop: 2 },
  searchWrapper: { paddingHorizontal: 12, paddingVertical: 8, zIndex: 100 },
  searchBox: { flexDirection: 'row', alignItems: 'center', borderRadius: 12, borderWidth: 1.5, paddingHorizontal: 12, paddingVertical: 9, gap: 8 },
  searchInput: { flex: 1, fontSize: 14 },
  resultsList: { marginTop: 4, borderRadius: 12, borderWidth: 1.5, maxHeight: 180, overflow: 'hidden' },
  resultItem: { paddingHorizontal: 14, paddingVertical: 11, borderBottomWidth: StyleSheet.hairlineWidth },
  resultText: { fontSize: 13 },
  filterWrapper: { paddingVertical: 8, borderBottomWidth: StyleSheet.hairlineWidth, borderBottomColor: 'rgba(0,0,0,0.08)' },
  filterScroll: { paddingHorizontal: 12, gap: 8 },
  chip: { paddingHorizontal: 14, paddingVertical: 6, borderRadius: 20, borderWidth: 1 },
  chipText: { fontSize: 13, fontWeight: '500' },
});
