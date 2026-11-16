import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useParams } from "next/navigation";
import { beforeEach, describe, expect, it, vi, type Mock } from "vitest";

import HomeError from "../app/[lang]/error";

vi.mock("next/navigation", () => ({
  useParams: vi.fn(),
}));

const useParamsMock = useParams as unknown as Mock;

describe("HomeError", () => {
  const reset = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(console, "error").mockImplementation(() => undefined);
    useParamsMock.mockReturnValue({ lang: "ru" });
  });

  it("shows a clear message and the retry action, not the technical detail", () => {
    const error = new Error("connect ECONNREFUSED 127.0.0.1:5054");
    render(<HomeError error={error} reset={reset} />);

    expect(screen.getByRole("heading", { level: 1, name: "Не удалось загрузить страницу" })).toBeInTheDocument();
    expect(screen.getByText("Проверьте соединение и попробуйте ещё раз.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Повторить" })).toBeInTheDocument();
    expect(screen.queryByText(/ECONNREFUSED/)).not.toBeInTheDocument();
  });

  it("calls reset when the retry button is pressed", async () => {
    const user = userEvent.setup();
    render(<HomeError error={new Error("boom")} reset={reset} />);

    await user.click(screen.getByRole("button", { name: "Повторить" }));

    expect(reset).toHaveBeenCalledTimes(1);
  });

  it("logs the technical detail once", () => {
    const error = new Error("boom");
    render(<HomeError error={error} reset={reset} />);

    expect(console.error).toHaveBeenCalledWith(error);
  });

  it("speaks English on the English page", () => {
    useParamsMock.mockReturnValue({ lang: "en" });
    render(<HomeError error={new Error("boom")} reset={reset} />);

    expect(screen.getByRole("heading", { level: 1, name: "The page could not be loaded" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Try again" })).toBeInTheDocument();
  });
});
