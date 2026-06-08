import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Linking,
} from 'react-native';
import { useThemeStore } from '../store/themeStore';
import { useLanguageStore } from '../stores/useLanguageStore';
import { LinearGradient } from 'expo-linear-gradient';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { router } from 'expo-router';
import { PhoneIcon, ArrowLeftIcon, TelegramIcon } from '../components/icons';

export default function HelpScreen() {
  const { colors } = useThemeStore();
  const { t } = useLanguageStore();
  const insets = useSafeAreaInsets();

  const handleCall = (phone: string) => {
    Linking.openURL(`tel:${phone}`);
  };

  const handleTelegram = () => {
    Linking.openURL('https://t.me/topilovasi_admin');
  };

  const faqs = [
    { q: t.help.faq1q, a: t.help.faq1a },
    { q: t.help.faq2q, a: t.help.faq2a },
    { q: t.help.faq3q, a: t.help.faq3a },
    { q: t.help.faq4q, a: t.help.faq4a },
  ];

  return (
    <View style={{ flex: 1, backgroundColor: colors.background }}>
      <LinearGradient
        colors={['#16A34A', '#15803D']}
        style={[styles.header, { paddingTop: insets.top + 12 }]}
      >
        <TouchableOpacity onPress={() => router.back()} style={styles.backBtn}>
          <ArrowLeftIcon size={22} color="#fff" />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>{t.help.title}</Text>
        <View style={{ width: 40 }} />
      </LinearGradient>

      <ScrollView
        contentContainerStyle={[styles.content, { paddingBottom: insets.bottom + 24 }]}
        showsVerticalScrollIndicator={false}
      >
        <Text style={[styles.sectionTitle, { color: colors.textPrimary }]}>
          {t.help.faqTitle}
        </Text>

        {faqs.map((item, index) => (
          <View key={index} style={[styles.faqCard, { backgroundColor: colors.card }]}>
            <Text style={[styles.faqQ, { color: colors.textPrimary }]}>{item.q}</Text>
            <Text style={[styles.faqA, { color: colors.textSecondary }]}>{item.a}</Text>
          </View>
        ))}

        <Text style={[styles.sectionTitle, { color: colors.textPrimary, marginTop: 24 }]}>
          {t.help.contactTitle}
        </Text>

        <TouchableOpacity
          style={[styles.contactCard, { backgroundColor: colors.card }]}
          onPress={() => handleCall('+998944627675')}
        >
          <View style={[styles.iconBox, { backgroundColor: '#16A34A22' }]}>
            <PhoneIcon size={22} color="#16A34A" />
          </View>
          <View>
            <Text style={[styles.contactLabel, { color: colors.textSecondary }]}>
              {t.help.phone}
            </Text>
            <Text style={[styles.contactValue, { color: colors.textPrimary }]}>
              +998 94 462 76 75
            </Text>
          </View>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.contactCard, { backgroundColor: colors.card }]}
          onPress={() => handleCall('+998701854741')}
        >
          <View style={[styles.iconBox, { backgroundColor: '#16A34A22' }]}>
            <PhoneIcon size={22} color="#16A34A" />
          </View>
          <View>
            <Text style={[styles.contactLabel, { color: colors.textSecondary }]}>
              {t.help.phone}
            </Text>
            <Text style={[styles.contactValue, { color: colors.textPrimary }]}>
              +998 70 185 47 41
            </Text>
          </View>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.contactCard, { backgroundColor: colors.card }]}
          onPress={handleTelegram}
        >
          <View style={[styles.iconBox, { backgroundColor: '#0088cc22' }]}>
            <TelegramIcon size={22} color="#0088cc" />
          </View>
          <View>
            <Text style={[styles.contactLabel, { color: colors.textSecondary }]}>
              Telegram
            </Text>
            <Text style={[styles.contactValue, { color: colors.textPrimary }]}>
              @topilovasi_admin
            </Text>
          </View>
        </TouchableOpacity>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingBottom: 16,
  },
  backBtn: {
    width: 40,
    height: 40,
    justifyContent: 'center',
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: '700',
    color: '#fff',
  },
  content: {
    padding: 16,
  },
  sectionTitle: {
    fontSize: 16,
    fontWeight: '700',
    marginBottom: 12,
  },
  faqCard: {
    borderRadius: 12,
    padding: 16,
    marginBottom: 10,
  },
  faqQ: {
    fontSize: 14,
    fontWeight: '600',
    marginBottom: 6,
  },
  faqA: {
    fontSize: 13,
    lineHeight: 20,
  },
  contactCard: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 14,
    borderRadius: 12,
    padding: 16,
    marginBottom: 10,
  },
  iconBox: {
    width: 44,
    height: 44,
    borderRadius: 22,
    justifyContent: 'center',
    alignItems: 'center',
  },
  contactLabel: {
    fontSize: 12,
    marginBottom: 2,
  },
  contactValue: {
    fontSize: 15,
    fontWeight: '600',
  },
});