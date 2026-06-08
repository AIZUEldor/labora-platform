import React, { useEffect, useRef, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
} from 'react-native';
import { WebView } from 'react-native-webview';
import * as Location from 'expo-location';
import { useThemeStore } from '../../store/themeStore';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { jobService } from '../../services/jobService';
import { NearbyJob } from '../../types';

export default function MapScreen() {
  const { colors } = useThemeStore();
  const insets = useSafeAreaInsets();
  const webViewRef = useRef<WebView>(null);

  const [loading, setLoading] = useState(true);
  const [locationError, setLocationError] = useState(false);
  const [userLocation, setUserLocation] = useState<{ lat: number; lng: number } | null>(null);
  const [jobs, setJobs] = useState<NearbyJob[]>([]);

  useEffect(() => {
    initMap();
  }, []);

  const initMap = async () => {
    setLoading(true);
    setLocationError(false);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        setLocationError(true);
        setLoading(false);
        return;
      }

      const location = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Balanced,
      });

      const lat = location.coords.latitude;
      const lng = location.coords.longitude;
      setUserLocation({ lat, lng });

      const nearbyJobs = await jobService.getNearbyJobs(lat, lng, 10);
      setJobs(nearbyJobs);
    } catch {
      setLocationError(true);
    } finally {
      setLoading(false);
    }
  };

  const getMapHtml = (lat: number, lng: number, jobs: NearbyJob[]): string => {
    const markersJs = jobs.map(job => {
      const title = job.title.replace(/'/g, "\\'").replace(/"/g, '&quot;');
      const city = (job.city ?? '').replace(/'/g, "\\'");
      return `
        (function() {
          var marker = L.marker([${job.latitude}, ${job.longitude}], { icon: jobIcon }).addTo(map);
          marker.bindPopup(
            '<div style="min-width:180px;font-family:sans-serif;padding:4px;">' +
            '<b style="font-size:14px;color:#111;">${title}</b><br/>' +
            '<span style="color:#16A34A;font-weight:600;">${job.salary.toLocaleString()} so\\'m</span><br/>' +
            '<span style="color:#666;font-size:12px;">${city}</span><br/>' +
            '<span style="color:#888;font-size:11px;">${job.distanceKm.toFixed(1)} km uzoqlikda</span><br/>' +
            '<button onclick="window.ReactNativeWebView.postMessage(\\'${job.id}\\')" ' +
            'style="margin-top:8px;background:#16A34A;color:#fff;border:none;' +
            'padding:6px 14px;border-radius:6px;cursor:pointer;font-size:13px;width:100%;">Ko\\'rish</button>' +
            '</div>'
          );
        })();
      `;
    }).join('\n');

    return `
<!DOCTYPE html>
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
    var map = L.map('map', { zoomControl: true }).setView([${lat}, ${lng}], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap',
      maxZoom: 19,
    }).addTo(map);

    var userIcon = L.divIcon({
      html: '<div style="width:18px;height:18px;background:#16A34A;border:3px solid #fff;border-radius:50%;box-shadow:0 0 0 5px rgba(22,163,74,0.25);"></div>',
      iconSize: [18, 18],
      iconAnchor: [9, 9],
      className: '',
    });

    L.marker([${lat}, ${lng}], { icon: userIcon })
      .addTo(map)
      .bindPopup('<b>Siz shu yerdasiz</b>');

    var jobIcon = L.divIcon({
      html: '<div style="width:16px;height:16px;background:#F59E0B;border:2px solid #fff;border-radius:50%;box-shadow:0 2px 6px rgba(0,0,0,0.3);"></div>',
      iconSize: [16, 16],
      iconAnchor: [8, 8],
      className: '',
    });

    ${markersJs}
  </script>
</body>
</html>
    `;
  };

  const handleWebViewMessage = (event: any) => {
    const jobId = event.nativeEvent.data;
    if (jobId) {
      router.push({ pathname: '/job-detail', params: { id: jobId } });
    }
  };

  if (loading) {
    return (
      <View style={[styles.centered, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color="#16A34A" />
        <Text style={[styles.loadingText, { color: colors.textSecondary }]}>
          Joylashuv aniqlanmoqda...
        </Text>
      </View>
    );
  }

  if (locationError) {
    return (
      <View style={[styles.centered, { backgroundColor: colors.background }]}>
        <Text style={[styles.errorText, { color: colors.textPrimary }]}>
          Joylashuv ruxsati berilmadi
        </Text>
        <TouchableOpacity style={styles.retryBtn} onPress={initMap}>
          <Text style={styles.retryText}>Qayta urinish</Text>
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
        <Text style={styles.headerTitle}>Yaqin atrofdagi ishlar</Text>
        <Text style={styles.headerSub}>{jobs.length} ta ish topildi</Text>
      </LinearGradient>

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
  centered: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 12,
  },
  loadingText: {
    fontSize: 14,
    marginTop: 8,
  },
  errorText: {
    fontSize: 16,
    fontWeight: '600',
    textAlign: 'center',
    paddingHorizontal: 32,
  },
  retryBtn: {
    backgroundColor: '#16A34A',
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 10,
  },
  retryText: {
    color: '#fff',
    fontWeight: '600',
    fontSize: 15,
  },
  header: {
    paddingHorizontal: 16,
    paddingBottom: 16,
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: '700',
    color: '#fff',
  },
  headerSub: {
    fontSize: 13,
    color: 'rgba(255,255,255,0.8)',
    marginTop: 2,
  },
});