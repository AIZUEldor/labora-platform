export const uz = {
  // Auth
  auth: {
    login: "Kirish",
    register: "Ro'yxatdan o'tish",
    phone: "Telefon raqam",
    password: "Parol",
    confirmPassword: "Parolni tasdiqlang",
    firstName: "Ism",
    lastName: "Familiya",
    forgotPassword: "Parolni unutdingizmi?",
    loginButton: "Kirish",
    registerButton: "Ro'yxatdan o'tish",
    haveAccount: "Akkauntingiz bormi?",
    noAccount: "Akkauntingiz yo'qmi?",
    role: "Rol",
    worker: "Ishchi",
    employer: "Ish beruvchi",
    phonePlaceholder: "+998 90 123 45 67",
  },

  // Navigation / Tabs
  tabs: {
    home: "Bosh sahifa",
    applications: "Arizalar",
    profile: "Profil",
    search: "Qidiruv",
    notifications: "Bildirishnomalar",
  },

  // Home
  home: {
    greeting: "Salom",
    findJob: "Ish toping",
    recentJobs: "So'nggi ishlar",
    nearbyJobs: "Yaqin ishlar",
    seeAll: "Barchasini ko'rish",
    noJobs: "Ishlar topilmadi",
    categories: "Kategoriyalar",
  },

  // Job
  job: {
    salary: "Maosh",
    location: "Manzil",
    category: "Kategoriya",
    description: "Tavsif",
    requirements: "Talablar",
    applyNow: "Ariza yuborish",
    applied: "Ariza yuborildi",
    uploadCV: "CV yuklash",
    deadline: "Muddat",
    jobType: "Ish turi",
    fullTime: "To'liq stavka",
    partTime: "Yarim stavka",
    remote: "Masofaviy",
    postedAt: "Joylashtirildi",
  },

  // Applications
  applications: {
    title: "Arizalarim",
    noApplications: "Arizalar yo'q",
    status: "Holat",
    pending: "Kutilmoqda",
    accepted: "Qabul qilindi",
    rejected: "Rad etildi",
    applied: "Yuborilgan",
    date: "Sana",
  },

  // Profile
  profile: {
    title: "Profil",
    editProfile: "Profilni tahrirlash",
    myCV: "Mening CV",
    uploadCV: "CV yuklash",
    settings: "Sozlamalar",
    language: "Til",
    theme: "Mavzu",
    darkMode: "Qorong'i rejim",
    lightMode: "Yorug' rejim",
    logout: "Chiqish",
    reviews: "Baholar",
    noReviews: "Baholar yo'q",
    phone: "Telefon",
    rating: "Reyting",
  },

  // Edit Profile
  editProfile: {
    title: "Profilni tahrirlash",
    firstName: "Ism",
    lastName: "Familiya",
    bio: "Haqida",
    skills: "Ko'nikmalar",
    experience: "Tajriba",
    save: "Saqlash",
    cancel: "Bekor qilish",
    avatar: "Rasm o'zgartirish",
    successMessage: "Profil muvaffaqiyatli yangilandi",
  },

  // Search
  search: {
    title: "Qidiruv",
    placeholder: "Ish nomi yoki kategoriya...",
    filters: "Filtrlar",
    sortBy: "Saralash",
    noResults: "Natija topilmadi",
    results: "natija",
  },

  // Notifications
  notifications: {
    title: "Bildirishnomalar",
    noNotifications: "Bildirishnomalar yo'q",
    markAllRead: "Barchasini o'qilgan deb belgilash",
    newApplication: "Yangi ariza",
    applicationAccepted: "Arizangiz qabul qilindi",
    applicationRejected: "Arizangiz rad etildi",
  },

  // Employer
  employer: {
    postJob: "Ish e'lon qilish",
    myJobs: "Mening e'lonlarim",
    applicants: "Arizachilar",
    noApplicants: "Arizachilar yo'q",
    acceptApplicant: "Qabul qilish",
    rejectApplicant: "Rad etish",
    jobTitle: "Ish nomi",
    jobDescription: "Tavsif",
    jobRequirements: "Talablar",
    minSalary: "Min maosh",
    maxSalary: "Maks maosh",
    publish: "E'lon qilish",
    draft: "Qoralama",
  },

  // Common
  common: {
    loading: "Yuklanmoqda...",
    error: "Xatolik yuz berdi",
    retry: "Qayta urinish",
    save: "Saqlash",
    cancel: "Bekor qilish",
    delete: "O'chirish",
    edit: "Tahrirlash",
    close: "Yopish",
    confirm: "Tasdiqlash",
    yes: "Ha",
    no: "Yo'q",
    ok: "OK",
    back: "Orqaga",
    next: "Keyingisi",
    done: "Tayyor",
    networkError: "Internet aloqasi yo'q",
    serverError: "Server xatosi",
    success: "Muvaffaqiyatli",
    somethingWentWrong: "Nimadir noto'g'ri ketdi",
    noData: "Ma'lumot yo'q",
    currency: "so'm",
  },

  // Language names (for language picker)
  languages: {
    uz: "O'zbekcha",
    ru: "Ruscha",
    en: "Inglizcha",
  },
};

export type TranslationKeys = typeof uz;
