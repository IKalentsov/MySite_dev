import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { HomePageView } from "@/_pages/home";

import {
  emptyContent,
  longDescriptionContent,
  longWordFixture,
  partialNoProfile,
  partialNoProjects,
  successContent,
} from "./testing/home-content";

describe("HomePageView", () => {
  describe("success state", () => {
    it("shows the site title, both section headings and the profile", () => {
      render(<HomePageView lang="ru" content={successContent} />);

      expect(screen.getByRole("heading", { level: 1, name: "MySite" })).toBeInTheDocument();
      expect(screen.getByRole("heading", { level: 2, name: "Обо мне" })).toBeInTheDocument();
      expect(screen.getByRole("heading", { level: 2, name: "Проекты" })).toBeInTheDocument();
      expect(screen.getByText("Full-stack .NET-разработчик")).toBeInTheDocument();
      expect(screen.getByText("Разрабатываю веб-приложения на .NET и React.")).toBeInTheDocument();
      expect(screen.queryByText("Страница загружена не полностью.")).not.toBeInTheDocument();
    });

    it("shows every project as a card with title, summary, stack, year and a link", () => {
      render(<HomePageView lang="ru" content={successContent} />);

      const cards = screen.getAllByRole("article");
      expect(cards).toHaveLength(successContent.projects.length);

      for (const project of successContent.projects) {
        const card = screen.getByRole("heading", { level: 3, name: project.title }).closest("article");
        expect(card).not.toBeNull();

        const cardScope = within(card as HTMLElement);
        expect(cardScope.getByText(project.summary)).toBeInTheDocument();
        for (const technology of project.stack) {
          expect(cardScope.getByText(technology)).toBeInTheDocument();
        }
        expect(cardScope.getByText(String(project.year))).toBeInTheDocument();
        expect(
          cardScope.getByRole("link", { name: `Открыть проект: ${project.title}` }),
        ).toHaveAttribute("href", project.link);
      }
    });

    it("shows the contact links", () => {
      render(<HomePageView lang="ru" content={successContent} />);

      expect(screen.getByRole("heading", { level: 2, name: "Связаться" })).toBeInTheDocument();
      expect(screen.getByRole("link", { name: "Telegram" })).toBeInTheDocument();
      expect(screen.getByRole("link", { name: "Почта" })).toHaveAttribute(
        "href",
        expect.stringContaining("mailto:"),
      );
    });
  });

  describe("empty state", () => {
    it("shows one empty message instead of the sections, and keeps the contacts", () => {
      render(<HomePageView lang="ru" content={emptyContent} />);

      expect(screen.getByRole("heading", { level: 1, name: "MySite" })).toBeInTheDocument();
      expect(screen.getByRole("heading", { level: 2, name: "Пока ничего нет" })).toBeInTheDocument();
      expect(screen.getByText("Здесь появятся текст «обо мне» и проекты.")).toBeInTheDocument();
      expect(screen.queryByRole("heading", { level: 2, name: "Обо мне" })).not.toBeInTheDocument();
      expect(screen.queryByRole("heading", { level: 2, name: "Проекты" })).not.toBeInTheDocument();
      expect(screen.getByRole("link", { name: "Telegram" })).toBeInTheDocument();
    });
  });

  describe("partial state: no profile, projects present", () => {
    it("shows what arrived and marks the missing part", () => {
      render(<HomePageView lang="ru" content={partialNoProfile} />);

      expect(screen.getByText("Страница загружена не полностью.")).toBeInTheDocument();
      expect(screen.getByText("Текст «обо мне» пока не заполнен.")).toBeInTheDocument();
      expect(screen.getByRole("heading", { level: 3, name: "MySite" })).toBeInTheDocument();
      expect(screen.queryByText(/Не удалось загрузить страницу/)).not.toBeInTheDocument();
    });
  });

  describe("partial state: profile present, no projects", () => {
    it("shows the profile and notes that there are no projects", () => {
      render(<HomePageView lang="ru" content={partialNoProjects} />);

      expect(screen.getByText("Страница загружена не полностью.")).toBeInTheDocument();
      expect(screen.getByText("Проектов пока нет.")).toBeInTheDocument();
      expect(screen.getByText("Full-stack .NET-разработчик")).toBeInTheDocument();
    });
  });

  describe("locale switch", () => {
    it("marks the current locale and links to the other one", () => {
      render(<HomePageView lang="ru" content={successContent} />);

      const nav = screen.getByRole("navigation", { name: "Переключатель языка" });
      expect(within(nav).getByRole("link", { name: "Русский" })).toHaveAttribute(
        "aria-current",
        "page",
      );
      expect(within(nav).getByRole("link", { name: "English" })).toHaveAttribute("href", "/en");
      expect(within(nav).getByRole("link", { name: "English" })).not.toHaveAttribute("aria-current");
    });

    it("renders English interface copy on the English page", () => {
      render(<HomePageView lang="en" content={successContent} />);

      expect(screen.getByRole("heading", { level: 2, name: "About me" })).toBeInTheDocument();
      expect(screen.getByRole("heading", { level: 2, name: "Projects" })).toBeInTheDocument();
      expect(screen.getByRole("link", { name: "Open project: MySite" })).toBeInTheDocument();

      const nav = screen.getByRole("navigation", { name: "Language switch" });
      expect(within(nav).getByRole("link", { name: "Russian" })).toHaveAttribute("href", "/ru");
      expect(within(nav).getByRole("link", { name: "English" })).toHaveAttribute(
        "aria-current",
        "page",
      );
    });
  });

  describe("long description", () => {
    it("renders the whole long word, so the text is wrapped rather than truncated", () => {
      render(<HomePageView lang="ru" content={longDescriptionContent} />);

      expect(screen.getByText(longWordFixture)).toBeInTheDocument();
    });
  });
});
