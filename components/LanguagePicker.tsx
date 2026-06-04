import React, { useState } from "react";
import {
  View,
  Text,
  TouchableOpacity,
  Modal,
  StyleSheet,
  Pressable,
} from "react-native";
import { useLanguageStore } from "../stores/useLanguageStore";
import { useThemeStore } from "../store/themeStore";
import { ThemeColors } from "../constants/colors";
import { Language } from "../i18n";
import { ChevronRightIcon, GlobeIcon } from "../components/icons";

const LANGUAGES: { code: Language; native: string; label: string }[] = [
  { code: "uz", native: "O'zbekcha", label: "Uzbek" },
  { code: "ru", native: "Русский",   label: "Russian" },
  { code: "en", native: "English",   label: "English" },
];

export const LanguagePicker = () => {
  const { language, setLanguage, t } = useLanguageStore();
  const { theme } = useThemeStore();
  const colors = ThemeColors[theme];
  const [visible, setVisible] = useState(false);

  const current = LANGUAGES.find((l) => l.code === language);

  return (
    <>
      <TouchableOpacity
        style={[styles.row, { borderBottomColor: colors.border }]}
        onPress={() => setVisible(true)}
        activeOpacity={0.7}
      >
        <View style={styles.left}>
          <GlobeIcon size={18} color={colors.textSecondary} />
          <Text style={[styles.label, { color: colors.textPrimary }]}>
            {t.profile.language}
          </Text>
        </View>
        <View style={styles.right}>
          <Text style={[styles.value, { color: colors.primary }]}>
            {current?.native}
          </Text>
          <ChevronRightIcon size={16} color={colors.textSecondary} />
        </View>
      </TouchableOpacity>

      <Modal
        visible={visible}
        transparent
        animationType="slide"
        onRequestClose={() => setVisible(false)}
      >
        <Pressable style={styles.overlay} onPress={() => setVisible(false)}>
          <Pressable
            style={[styles.sheet, { backgroundColor: colors.card }]}
            onPress={() => {}}
          >
            <View style={[styles.handle, { backgroundColor: colors.border }]} />

            <Text style={[styles.sheetTitle, { color: colors.textPrimary }]}>
              {t.profile.language}
            </Text>

            {LANGUAGES.map((lang) => {
              const selected = lang.code === language;
              return (
                <TouchableOpacity
                  key={lang.code}
                  style={[
                    styles.langItem,
                    {
                      borderColor: selected ? colors.primary : colors.border,
                      backgroundColor: selected
                        ? colors.primary + "15"
                        : "transparent",
                    },
                  ]}
                  onPress={() => {
                    setLanguage(lang.code);
                    setVisible(false);
                  }}
                  activeOpacity={0.7}
                >
                  <Text style={[styles.langNative, { color: colors.textPrimary }]}>
                    {lang.native}
                  </Text>
                  <Text style={[styles.langSub, { color: colors.textSecondary }]}>
                    {lang.label}
                  </Text>
                  {selected && (
                    <View
                      style={[
                        styles.check,
                        { backgroundColor: colors.primary },
                      ]}
                    >
                      <Text style={styles.checkText}>✓</Text>
                    </View>
                  )}
                </TouchableOpacity>
              );
            })}
          </Pressable>
        </Pressable>
      </Modal>
    </>
  );
};

const styles = StyleSheet.create({
  row: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    paddingVertical: 14,
    borderBottomWidth: StyleSheet.hairlineWidth,
  },
  left: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
  },
  label: {
    fontSize: 15,
    fontWeight: "500",
  },
  right: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
  },
  value: {
    fontSize: 14,
    fontWeight: "600",
  },
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.45)",
    justifyContent: "flex-end",
  },
  sheet: {
    borderTopLeftRadius: 24,
    borderTopRightRadius: 24,
    padding: 24,
    paddingBottom: 48,
  },
  handle: {
    width: 36,
    height: 4,
    borderRadius: 2,
    alignSelf: "center",
    marginBottom: 20,
  },
  sheetTitle: {
    fontSize: 18,
    fontWeight: "700",
    marginBottom: 16,
  },
  langItem: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 14,
    paddingHorizontal: 16,
    borderRadius: 12,
    borderWidth: 1.5,
    marginBottom: 10,
  },
  langNative: {
    fontSize: 16,
    fontWeight: "600",
    flex: 1,
  },
  langSub: {
    fontSize: 13,
    marginRight: 8,
  },
  check: {
    width: 22,
    height: 22,
    borderRadius: 11,
    alignItems: "center",
    justifyContent: "center",
  },
  checkText: {
    color: "#fff",
    fontSize: 13,
    fontWeight: "700",
  },
});
