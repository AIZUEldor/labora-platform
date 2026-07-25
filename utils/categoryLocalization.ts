// Display-only category/subcategory name translations, keyed by the exact backend-returned
// `name` string. This never changes what's stored or submitted - categoryId, categoryName,
// subCategoryId, and subCategoryName must always keep using the raw Category.name from the
// backend; only the rendered label is translated here, at render time.

export interface CategoryTranslation {
  uz: string;
  ru: string;
  en: string;
}

export const CATEGORY_NAMES: Record<string, CategoryTranslation> = {
  'Daily':         { uz: 'Kunlik ishlar',       ru: 'Ежедневная работа',  en: 'Daily jobs' },
  'IT':            { uz: 'IT',                  ru: 'ИТ',                 en: 'IT' },
  'Construction':  { uz: 'Qurilish',            ru: 'Строительство',      en: 'Construction' },
  'Driver':        { uz: 'Haydovchi',           ru: 'Водитель',           en: 'Driver' },
  'Chef':          { uz: 'Oshpaz',              ru: 'Повар',              en: 'Chef' },
  'Medical':       { uz: 'Tibbiyot',            ru: 'Медицина',           en: 'Medical' },
  'Education':     { uz: "Ta'lim",              ru: 'Образование',        en: 'Education' },
  'Finance':       { uz: 'Moliya',              ru: 'Финансы',            en: 'Finance' },
  'Security':      { uz: 'Qorovul',             ru: 'Охрана',             en: 'Security' },
  'Cleaning':      { uz: 'Tozalik',             ru: 'Уборка',             en: 'Cleaning' },
  'Design':        { uz: 'Dizayn',              ru: 'Дизайн',             en: 'Design' },
  'Marketing':     { uz: 'Marketing',           ru: 'Маркетинг',          en: 'Marketing' },
  'Warehouse':     { uz: 'Ombor',               ru: 'Склад',              en: 'Warehouse' },
  'Trade':         { uz: 'Savdo',               ru: 'Торговля',           en: 'Trade' },
  'Agriculture':   { uz: "Qishloq xo'jaligi",  ru: 'Сельское хозяйство', en: 'Agriculture' },
  'Manufacturing': { uz: 'Ishlab chiqarish',    ru: 'Производство',       en: 'Manufacturing' },
  'Courier':       { uz: 'Kuryer',              ru: 'Курьер',             en: 'Courier' },
  'Legal':         { uz: 'Huquq',               ru: 'Юридические услуги', en: 'Legal' },
  'HR':            { uz: 'Kadrlar',             ru: 'Кадры',              en: 'HR' },
  'Real Estate':   { uz: "Ko'chmas mulk",       ru: 'Недвижимость',       en: 'Real Estate' },
  'Beauty':        { uz: "Go'zallik",           ru: 'Красота',            en: 'Beauty' },
  'Auto Service':  { uz: "Avto ta'mir",         ru: 'Авто сервис',        en: 'Auto Service' },
  'Textile':       { uz: "To'qimachilik",       ru: 'Текстиль',           en: 'Textile' },

  // Subcategories - seeded in Uzbek only (Labora.Infrastructure/Migrations/
  // 20260602000311_SeedSubCategories.cs), so `uz` below is the raw name itself.
  // IT
  'Veb-dasturchi':          { uz: 'Veb-dasturchi',          ru: 'Веб-разработчик',            en: 'Web developer' },
  'Mobil dasturchi':        { uz: 'Mobil dasturchi',        ru: 'Мобильный разработчик',       en: 'Mobile developer' },
  '1C dasturchi':           { uz: '1C dasturchi',           ru: 'Программист 1С',              en: '1C developer' },
  'Tarmoq muhandisi':       { uz: 'Tarmoq muhandisi',       ru: 'Сетевой инженер',             en: 'Network engineer' },
  'UI/UX dizayner':         { uz: 'UI/UX dizayner',         ru: 'UI/UX дизайнер',              en: 'UI/UX designer' },
  'AI mutaxassis':          { uz: 'AI mutaxassis',          ru: 'Специалист по ИИ',            en: 'AI specialist' },
  // Construction
  "Usta (umumiy)":          { uz: "Usta (umumiy)",          ru: 'Мастер (универсал)',          en: 'Handyman (general)' },
  'Elektrik':               { uz: 'Elektrik',               ru: 'Электрик',                    en: 'Electrician' },
  'Santexnik':              { uz: 'Santexnik',              ru: 'Сантехник',                   en: 'Plumber' },
  "Bo'yoqchi":              { uz: "Bo'yoqchi",              ru: 'Маляр',                       en: 'Painter' },
  'Gipschi':                { uz: 'Gipschi',                ru: 'Гипсокартонщик',              en: 'Drywall installer' },
  'Kafel ustasi':           { uz: 'Kafel ustasi',           ru: 'Плиточник',                   en: 'Tiler' },
  'Metall konstruksiya':    { uz: 'Metall konstruksiya',    ru: 'Металлоконструкции',          en: 'Metal structures' },
  // Driver
  'Taksi haydovchi':        { uz: 'Taksi haydovchi',        ru: 'Водитель такси',              en: 'Taxi driver' },
  'Yuk haydovchi':          { uz: 'Yuk haydovchi',          ru: 'Водитель грузовика',          en: 'Truck driver' },
  'Avtobus haydovchi':      { uz: 'Avtobus haydovchi',      ru: 'Водитель автобуса',           en: 'Bus driver' },
  'Shaxsiy haydovchi':      { uz: 'Shaxsiy haydovchi',      ru: 'Личный водитель',             en: 'Personal driver' },
  'Kuryer haydovchi':       { uz: 'Kuryer haydovchi',       ru: 'Водитель-курьер',             en: 'Courier driver' },
  // Chef
  'Konditer':               { uz: 'Konditer',               ru: 'Кондитер',                    en: 'Confectioner' },
  'Barista':                { uz: 'Barista',                ru: 'Бариста',                     en: 'Barista' },
  'Ofitsiant':              { uz: 'Ofitsiant',              ru: 'Официант',                    en: 'Waiter' },
  'Sushi master':           { uz: 'Sushi master',           ru: 'Суши-мастер',                 en: 'Sushi chef' },
  'Tandirchi':              { uz: 'Tandirchi',              ru: 'Тандырщик',                   en: 'Tandoor baker' },
  // Medical
  'Vrach':                  { uz: 'Vrach',                  ru: 'Врач',                        en: 'Doctor' },
  'Hamshira':               { uz: 'Hamshira',               ru: 'Медсестра',                   en: 'Nurse' },
  'Dorixonachi':            { uz: 'Dorixonachi',            ru: 'Фармацевт',                   en: 'Pharmacist' },
  'Stomatolog':             { uz: 'Stomatolog',             ru: 'Стоматолог',                  en: 'Dentist' },
  'Psixolog':               { uz: 'Psixolog',               ru: 'Психолог',                    en: 'Psychologist' },
  'Laborant':                { uz: 'Laborant',              ru: 'Лаборант',                    en: 'Lab technician' },
  // Education
  "Maktab o'qituvchi":      { uz: "Maktab o'qituvchi",      ru: 'Школьный учитель',            en: 'School teacher' },
  'Repetitor':              { uz: 'Repetitor',              ru: 'Репетитор',                   en: 'Tutor' },
  'Ingliz tili o\'qituvchi':{ uz: 'Ingliz tili o\'qituvchi',ru: 'Учитель английского языка',   en: 'English teacher' },
  'Sport murabbiy':         { uz: 'Sport murabbiy',         ru: 'Спортивный тренер',           en: 'Sports coach' },
  'Tarbiyachi':             { uz: 'Tarbiyachi',             ru: 'Воспитатель',                 en: 'Kindergarten teacher' },
  "Online o'qituvchi":      { uz: "Online o'qituvchi",      ru: 'Онлайн-преподаватель',        en: 'Online teacher' },
  // Finance
  'Buxgalter':              { uz: 'Buxgalter',              ru: 'Бухгалтер',                   en: 'Accountant' },
  '1C operator':            { uz: '1C operator',            ru: 'Оператор 1С',                 en: '1C operator' },
  'Kassir':                 { uz: 'Kassir',                 ru: 'Кассир',                      en: 'Cashier' },
  'Moliyaviy tahlilchi':    { uz: 'Moliyaviy tahlilchi',    ru: 'Финансовый аналитик',         en: 'Financial analyst' },
  'Auditor':                { uz: 'Auditor',                ru: 'Аудитор',                     en: 'Auditor' },
  // Security
  'Qorovul':                { uz: 'Qorovul',                ru: 'Охранник',                    en: 'Guard' },
  'Xavfsizlik xodimi':      { uz: 'Xavfsizlik xodimi',      ru: 'Сотрудник безопасности',      en: 'Security officer' },
  'CCTV operator':          { uz: 'CCTV operator',          ru: 'Оператор видеонаблюдения',    en: 'CCTV operator' },
  // Cleaning
  'Farrosh':                { uz: 'Farrosh',                ru: 'Уборщик',                     en: 'Cleaner' },
  'Tozalash brigadasi':     { uz: 'Tozalash brigadasi',     ru: 'Бригада уборки',              en: 'Cleaning crew' },
  "Bog'bon":                { uz: "Bog'bon",                ru: 'Садовник',                    en: 'Gardener' },
  'Oyna tozalovchi':        { uz: 'Oyna tozalovchi',        ru: 'Мойщик окон',                 en: 'Window cleaner' },
  // Design
  'Grafik dizayner':        { uz: 'Grafik dizayner',        ru: 'Графический дизайнер',        en: 'Graphic designer' },
  'Video montajchi':        { uz: 'Video montajchi',        ru: 'Видеомонтажёр',               en: 'Video editor' },
  '3D dizayner':            { uz: '3D dizayner',            ru: '3D-дизайнер',                 en: '3D designer' },
  'Fotograf':               { uz: 'Fotograf',               ru: 'Фотограф',                    en: 'Photographer' },
  'Kontent yaratuvchi':     { uz: 'Kontent yaratuvchi',     ru: 'Создатель контента',          en: 'Content creator' },
  // Marketing
  'SMM mutaxassis':         { uz: 'SMM mutaxassis',         ru: 'SMM-специалист',              en: 'SMM specialist' },
  'SEO mutaxassis':         { uz: 'SEO mutaxassis',         ru: 'SEO-специалист',              en: 'SEO specialist' },
  'Targetolog':             { uz: 'Targetolog',             ru: 'Таргетолог',                  en: 'Targeting specialist' },
  'PR mutaxassis':          { uz: 'PR mutaxassis',          ru: 'PR-специалист',               en: 'PR specialist' },
  'Brand menejeri':         { uz: 'Brand menejeri',         ru: 'Бренд-менеджер',              en: 'Brand manager' },
  // Warehouse
  'Omborchi':               { uz: 'Omborchi',               ru: 'Кладовщик',                   en: 'Warehouse keeper' },
  "Yuk ko'taruvchi":        { uz: "Yuk ko'taruvchi",        ru: 'Грузчик',                     en: 'Loader' },
  'Inventarizatsiya':       { uz: 'Inventarizatsiya',       ru: 'Инвентаризация',              en: 'Inventory clerk' },
  'Logist':                 { uz: 'Logist',                 ru: 'Логист',                      en: 'Logistics specialist' },
  // Daily
  'Uy yumushi':             { uz: 'Uy yumushi',             ru: 'Домашние дела',               en: 'Housework' },
  "Ko'chirish":             { uz: "Ko'chirish",             ru: 'Переезд',                     en: 'Moving/relocation' },
  "Mayda ta'mir":           { uz: "Mayda ta'mir",           ru: 'Мелкий ремонт',               en: 'Minor repairs' },
  'Yuk tashish':            { uz: 'Yuk tashish',            ru: 'Переноска грузов',            en: 'Load carrying' },
  // Trade
  'Sotuvchi':               { uz: 'Sotuvchi',               ru: 'Продавец',                    en: 'Salesperson' },
  'Savdo vakili':           { uz: 'Savdo vakili',           ru: 'Торговый представитель',      en: 'Sales representative' },
  'Online savdo':           { uz: 'Online savdo',           ru: 'Онлайн-продажи',              en: 'Online sales' },
  "Do'kon menejeri":        { uz: "Do'kon menejeri",        ru: 'Менеджер магазина',           en: 'Store manager' },
  // Agriculture
  'Dehqon':                 { uz: 'Dehqon',                 ru: 'Фермер',                      en: 'Farmer' },
  'Chorvachilik':           { uz: 'Chorvachilik',           ru: 'Животноводство',              en: 'Livestock farming' },
  "Bog'dorchilik":          { uz: "Bog'dorchilik",          ru: 'Садоводство',                 en: 'Horticulture' },
  'Parrandachilik':         { uz: 'Parrandachilik',         ru: 'Птицеводство',                en: 'Poultry farming' },
  'Traktorchi':             { uz: 'Traktorchi',             ru: 'Тракторист',                  en: 'Tractor operator' },
  // Manufacturing
  'Zavod ishchisi':         { uz: 'Zavod ishchisi',         ru: 'Заводской рабочий',           en: 'Factory worker' },
  'Mashina operatori':      { uz: 'Mashina operatori',      ru: 'Оператор станка',             en: 'Machine operator' },
  'Sifat nazorati':         { uz: 'Sifat nazorati',         ru: 'Контроль качества',           en: 'Quality control' },
  'Elektr montajchi':       { uz: 'Elektr montajchi',       ru: 'Электромонтажник',            en: 'Electrical installer' },
  // Courier
  'Piyoda kuryer':          { uz: 'Piyoda kuryer',          ru: 'Пеший курьер',                en: 'Foot courier' },
  'Mototsikl kuryer':       { uz: 'Mototsikl kuryer',       ru: 'Курьер на мотоцикле',         en: 'Motorbike courier' },
  'Avtomobil kuryer':       { uz: 'Avtomobil kuryer',       ru: 'Курьер на автомобиле',        en: 'Car courier' },
  'Express yetkazish':      { uz: 'Express yetkazish',      ru: 'Экспресс-доставка',           en: 'Express delivery' },
  // Legal
  'Advokat':                { uz: 'Advokat',                ru: 'Адвокат',                     en: 'Lawyer' },
  'Yurist':                 { uz: 'Yurist',                 ru: 'Юрист',                       en: 'Legal counsel' },
  'Notarius yordamchi':     { uz: 'Notarius yordamchi',     ru: 'Помощник нотариуса',          en: 'Notary assistant' },
  'HR-yurist':              { uz: 'HR-yurist',              ru: 'HR-юрист',                    en: 'HR lawyer' },
  // HR
  'HR menejeri':            { uz: 'HR menejeri',            ru: 'HR-менеджер',                 en: 'HR manager' },
  'Rekruter':               { uz: 'Rekruter',               ru: 'Рекрутер',                    en: 'Recruiter' },
  'Kadrlar mutaxassisi':    { uz: 'Kadrlar mutaxassisi',    ru: 'Специалист по кадрам',        en: 'HR specialist' },
  // Real Estate
  'Rieltor':                { uz: 'Rieltor',                ru: 'Риелтор',                     en: 'Realtor' },
  'Mulk bahovchisi':        { uz: 'Mulk bahovchisi',        ru: 'Оценщик недвижимости',        en: 'Property appraiser' },
  'Ijara menejeri':         { uz: 'Ijara menejeri',         ru: 'Менеджер по аренде',          en: 'Rental manager' },
  // Beauty
  'Sartarosh':              { uz: 'Sartarosh',              ru: 'Парикмахер',                  en: 'Barber' },
  'Kosmetolog':             { uz: 'Kosmetolog',             ru: 'Косметолог',                  en: 'Cosmetologist' },
  'Manikur/Pedikur':        { uz: 'Manikur/Pedikur',        ru: 'Маникюр/Педикюр',             en: 'Manicure/Pedicure' },
  'Vizajist':               { uz: 'Vizajist',               ru: 'Визажист',                    en: 'Makeup artist' },
  'Massajchi':              { uz: 'Massajchi',              ru: 'Массажист',                   en: 'Masseur' },
  // Auto Service
  'Avto mexanik':           { uz: 'Avto mexanik',           ru: 'Автомеханик',                 en: 'Auto mechanic' },
  'Shinachi':               { uz: 'Shinachi',               ru: 'Шиномонтажник',               en: 'Tire specialist' },
  'Avto elektrik':          { uz: 'Avto elektrik',          ru: 'Автоэлектрик',                en: 'Auto electrician' },
  "Avto bo'yoqchi":         { uz: "Avto bo'yoqchi",         ru: 'Автомаляр',                   en: 'Auto painter' },
  // Textile
  'Tikuvchi':               { uz: 'Tikuvchi',               ru: 'Швея',                        en: 'Seamstress' },
  'Kashtachi':              { uz: 'Kashtachi',              ru: 'Вышивальщица',                en: 'Embroiderer' },
  'Kiyim dizayneri':        { uz: 'Kiyim dizayneri',        ru: 'Модельер',                    en: 'Fashion designer' },
  'Fabrika ishchisi':       { uz: 'Fabrika ishchisi',       ru: 'Работник фабрики',            en: 'Factory worker' },
};

// Returns a localized display label for a backend category/subcategory `name`, falling back to
// the original name unchanged when no translation exists (new/unlisted categories).
export function getCategoryLabel(name: string | null | undefined, language: string): string {
  if (!name) return '';
  const translation = CATEGORY_NAMES[name];
  if (!translation) return name;
  return language === 'uz' ? translation.uz : language === 'ru' ? translation.ru : translation.en;
}
