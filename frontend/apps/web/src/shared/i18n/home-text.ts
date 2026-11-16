import type { Locale } from "../config/locales";

/**
 * Interface copy of the home page, one dictionary per locale. Content (the profile
 * and the projects) arrives from the API already resolved to the requested locale;
 * these strings are only the page's own labels and state messages.
 */
export interface HomeTexts {
  pageTitle: string;
  aboutHeading: string;
  projectsHeading: string;
  contactHeading: string;
  noProfileNote: string;
  noProjectsNote: string;
  emptyStateTitle: string;
  emptyStateHint: string;
  partialLoadLabel: string;
  errorTitle: string;
  errorMessage: string;
  retryButton: string;
  contactTelegramLabel: string;
  contactEmailLabel: string;
  openProject: string;
  languageSwitch: string;
  localeRu: string;
  localeEn: string;
}

const ru: HomeTexts = {
  pageTitle: "MySite",
  aboutHeading: "Обо мне",
  projectsHeading: "Проекты",
  contactHeading: "Связаться",
  noProfileNote: "Текст «обо мне» пока не заполнен.",
  noProjectsNote: "Проектов пока нет.",
  emptyStateTitle: "Пока ничего нет",
  emptyStateHint: "Здесь появятся текст «обо мне» и проекты.",
  partialLoadLabel: "Страница загружена не полностью.",
  errorTitle: "Не удалось загрузить страницу",
  errorMessage: "Проверьте соединение и попробуйте ещё раз.",
  retryButton: "Повторить",
  contactTelegramLabel: "Telegram",
  contactEmailLabel: "Почта",
  openProject: "Открыть проект",
  languageSwitch: "Переключатель языка",
  localeRu: "Русский",
  localeEn: "English",
};

const en: HomeTexts = {
  pageTitle: "MySite",
  aboutHeading: "About me",
  projectsHeading: "Projects",
  contactHeading: "Contact",
  noProfileNote: "The “about me” text has not been written yet.",
  noProjectsNote: "No projects yet.",
  emptyStateTitle: "Nothing here yet",
  emptyStateHint: "The “about me” text and the projects will appear here.",
  partialLoadLabel: "This page is only partly filled in.",
  errorTitle: "The page could not be loaded",
  errorMessage: "Check your connection and try again.",
  retryButton: "Try again",
  contactTelegramLabel: "Telegram",
  contactEmailLabel: "Email",
  openProject: "Open project",
  languageSwitch: "Language switch",
  localeRu: "Russian",
  localeEn: "English",
};

const dictionary: Record<Locale, HomeTexts> = { ru, en };

export function getHomeText(locale: Locale): HomeTexts {
  return dictionary[locale];
}
