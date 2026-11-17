import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import Loading from "@/app/[lang]/loading";

describe("home route loading state", () => {
  it("marks the screen as busy and hides the skeleton from assistive technology", () => {
    const { container } = render(<Loading />);

    expect(container.querySelector('[aria-busy="true"]')).not.toBeNull();
    expect(container.querySelectorAll('[aria-hidden="true"]').length).toBeGreaterThan(0);
    expect(container.querySelector("h1")).toBeNull();
  });
});
