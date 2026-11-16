import { expect, test } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { Button } from "@mysite/ui";

test("renders a button with the given text", () => {
  render(<Button>Click me</Button>);
  expect(screen.getByRole("button", { name: "Click me" })).toBeInTheDocument();
});

test("calls onClick when clicked", async () => {
  const onClick = vi.fn();
  render(<Button onClick={onClick}>Submit</Button>);
  await userEvent.click(screen.getByRole("button", { name: "Submit" }));
  expect(onClick).toHaveBeenCalledTimes(1);
});

test("renders with destructive variant", () => {
  render(<Button variant="destructive">Delete</Button>);
  const button = screen.getByRole("button", { name: "Delete" });
  expect(button).toHaveClass("bg-destructive");
});
