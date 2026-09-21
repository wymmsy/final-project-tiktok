#!/usr/bin/env python3
"""
Скрипт-линтер для TikTokFeed (.NET, Clean Architecture: Domain -> Application -> Infrastructure -> Api).

Возвращает 1, если найдено хотя бы одно нарушение уровня "error", иначе 0.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
from dataclasses import dataclass, asdict
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent

# каталоги которые не сканируем
EXCLUDED_DIR_NAMES = {"bin", "obj", ".git", "node_modules", ".vs", ".idea"}

# контексты проекта
CONTEXTS = ("Content", "Engagement", "Identity")
LAYERS = ("Domain", "Application", "Infrastructure", "Api")

LAYER_DIR_RE = re.compile(r"(?:^|[\\/])(?:%s)\.(%s)(?:[\\/]|$)" % (
    "|".join(CONTEXTS), "|".join(LAYERS)
))


@dataclass
class Violation:
    rule: str
    severity: str  # error/warning/info
    file: str
    line: int
    message: str


VIOLATIONS: list[Violation] = []


def add(rule: str, severity: str, path: Path, line: int, message: str) -> None:
    VIOLATIONS.append(
        Violation(
            rule=rule,
            severity=severity,
            file=str(path.relative_to(ROOT)).replace("\\", "/"),
            line=line,
            message=message,
        )
    )


def iter_files(base: Path, suffix: str) -> list[Path]:
    result = []
    for dirpath, dirnames, filenames in os.walk(base):
        dirnames[:] = [d for d in dirnames if d not in EXCLUDED_DIR_NAMES]
        for name in filenames:
            if name.endswith(suffix):
                result.append(Path(dirpath) / name)
    return result


def layer_of(path: Path) -> str | None:
    match = LAYER_DIR_RE.search(str(path))
    return match.group(1) if match else None


def read_lines(path: Path) -> list[str]:
    try:
        return path.read_text(encoding="utf-8-sig").splitlines()
    except UnicodeDecodeError:
        return path.read_text(encoding="latin-1").splitlines()


# Правило 1: Domain не должен зависеть от Application/Infrastructure/Api
DOMAIN_FORBIDDEN_USINGS = re.compile(
    r"^\s*using\s+(?:[A-Za-z0-9_.]*\.(?:Application|Infrastructure|Api)"
    r"|Microsoft\.EntityFrameworkCore"
    r"|Microsoft\.AspNetCore)"
)


def check_domain_layering(cs_files: list[Path]) -> None:
    for path in cs_files:
        if layer_of(path) != "Domain":
            continue
        for i, line in enumerate(read_lines(path), start=1):
            if DOMAIN_FORBIDDEN_USINGS.match(line):
                add(
                    "domain-layer-isolation",
                    "error",
                    path,
                    i,
                    f"Domain-слой ссылается на внешний слой/фреймворк: '{line.strip()}'. "
                    "Domain должен оставаться независимым от Application/Infrastructure/Api и EF Core/ASP.NET Core.",
                )


# Правило 2: Application не должен зависеть от Infrastructure/Api
APPLICATION_FORBIDDEN_USINGS = re.compile(
    r"^\s*using\s+(?:[A-Za-z0-9_.]*\.(?:Infrastructure|Api)"
    r"|Microsoft\.AspNetCore\.Mvc)"
)


def check_application_layering(cs_files: list[Path]) -> None:
    for path in cs_files:
        if layer_of(path) != "Application":
            continue
        for i, line in enumerate(read_lines(path), start=1):
            if APPLICATION_FORBIDDEN_USINGS.match(line):
                add(
                    "application-layer-isolation",
                    "error",
                    path,
                    i,
                    f"Application-слой ссылается на Infrastructure/Api: '{line.strip()}'. "
                    "Application должен зависеть только от Domain.",
                )


# Правило 3: контроллеры не должны напрямую использовать DbContext —
DBCONTEXT_RE = re.compile(r"\bDbContext\b")


def check_controllers_no_dbcontext(cs_files: list[Path]) -> None:
    for path in cs_files:
        if "Controllers" not in path.parts and "Grpc" not in str(path):
            continue
        for i, line in enumerate(read_lines(path), start=1):
            if DBCONTEXT_RE.search(line):
                add(
                    "no-dbcontext-in-controller",
                    "error",
                    path,
                    i,
                    "Контроллер/gRPC-сервис напрямую использует DbContext вместо репозитория или сервиса приложения.",
                )


# Правило 4: запрет Console.WriteLine
CONSOLE_WRITE_RE = re.compile(r"\bConsole\.(Write|WriteLine)\s*\(")


def check_console_writeline(cs_files: list[Path]) -> None:
    for path in cs_files:
        for i, line in enumerate(read_lines(path), start=1):
            if CONSOLE_WRITE_RE.search(line):
                add(
                    "no-console-write",
                    "warning",
                    path,
                    i,
                    "Использование Console.Write/WriteLine вместо ILogger.",
                )


# Правило 5: пустые catch-блоки недопустимы
EMPTY_CATCH_RE = re.compile(r"catch\s*(?:\([^)]*\))?\s*\{\s*\}")


def check_empty_catch(cs_files: list[Path]) -> None:
    for path in cs_files:
        text = "\n".join(read_lines(path))
        for match in EMPTY_CATCH_RE.finditer(text):
            line_no = text.count("\n", 0, match.start()) + 1
            add(
                "empty-catch-block",
                "error",
                path,
                line_no,
                "Пустой catch-блок молча проглатывает исключение.",
            )


# Правило 6: незакрытые TODO/FIXME/HACK в коде
TODO_RE = re.compile(r"//\s*(TODO|FIXME|HACK)\b", re.IGNORECASE)


def check_todo_comments(cs_files: list[Path]) -> None:
    for path in cs_files:
        for i, line in enumerate(read_lines(path), start=1):
            m = TODO_RE.search(line)
            if m:
                add(
                    "todo-comment",
                    "info",
                    path,
                    i,
                    f"Незакрытая заметка {m.group(1).upper()} в коде: '{line.strip()}'",
                )


# Правило 7: async void — исключения невозможно перехватить
ASYNC_VOID_RE = re.compile(r"\basync\s+void\b")


def check_async_void(cs_files: list[Path]) -> None:
    for path in cs_files:
        for i, line in enumerate(read_lines(path), start=1):
            if ASYNC_VOID_RE.search(line):
                add(
                    "no-async-void",
                    "error",
                    path,
                    i,
                    "Метод объявлен как 'async void' — используйте 'async Task'.",
                )


# Правило 8: sync-over-async — блокирующее ожидание Task через .Result/.Wait()
SYNC_OVER_ASYNC_RE = re.compile(r"\.(Result|Wait)\s*\(?(?!.*outcome)")


def check_sync_over_async(cs_files: list[Path]) -> None:
    for path in cs_files:
        for i, line in enumerate(read_lines(path), start=1):
            stripped = line.strip()
            if not stripped or stripped.startswith("//"):
                continue
            if re.search(r"\.Result\b", line) or re.search(r"\.Wait\(\)", line):
                add(
                    "sync-over-async",
                    "warning",
                    path,
                    i,
                    f"Похоже на блокирующее ожидание Task ('.Result'/'.Wait()'): '{stripped}'. Проверьте вручную.",
                )


# Правило 9: интерфейсы должны называться с префиксом 'I'
INTERFACE_DECL_RE = re.compile(r"\binterface\s+([A-Za-z0-9_]+)")


def check_interface_naming(cs_files: list[Path]) -> None:
    for path in cs_files:
        for i, line in enumerate(read_lines(path), start=1):
            m = INTERFACE_DECL_RE.search(line)
            if m and not re.match(r"^I[A-Z0-9]", m.group(1)):
                add(
                    "interface-naming",
                    "warning",
                    path,
                    i,
                    f"Интерфейс '{m.group(1)}' не соответствует конвенции именования 'I<Name>'.",
                )


# Правило 10: захардкоженные пароли/секреты в appsettings*.json
SECRET_KEY_RE = re.compile(
    r'"(?P<key>[^"]*(?:password|secret|apikey|api_key)[^"]*)"\s*:\s*"(?P<value>[^"]+)"',
    re.IGNORECASE,
)
CONN_STRING_PASSWORD_RE = re.compile(r"Password\s*=\s*[^;\"]+", re.IGNORECASE)


def check_hardcoded_secrets(json_files: list[Path]) -> None:
    for path in json_files:
        if "bin" in path.parts or "obj" in path.parts:
            continue
        for i, line in enumerate(read_lines(path), start=1):
            if SECRET_KEY_RE.search(line) or CONN_STRING_PASSWORD_RE.search(line):
                add(
                    "hardcoded-secret",
                    "info",
                    path,
                    i,
                    "Похоже на захардкоженный пароль/секрет в конфиге. "
                    "Для dev-окружения допустимо, но убедитесь, что это не попадает в prod.",
                )


CHECKS = [
    ("Изоляция Domain-слоя", check_domain_layering),
    ("Изоляция Application-слоя", check_application_layering),
    ("DbContext вне Infrastructure", check_controllers_no_dbcontext),
    ("Console.WriteLine вместо ILogger", check_console_writeline),
    ("Пустые catch-блоки", check_empty_catch),
    ("TODO/FIXME/HACK комментарии", check_todo_comments),
    ("async void методы", check_async_void),
    ("Sync-over-async (.Result/.Wait())", check_sync_over_async),
    ("Именование интерфейсов", check_interface_naming),
    ("Захардкоженные секреты в конфигах", check_hardcoded_secrets),
]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--json", action="store_true", help="вывести результат в формате JSON")
    parser.add_argument(
        "--path",
        default=str(ROOT / "src"),
        help="каталог для сканирования (по умолчанию src/)",
    )
    args = parser.parse_args()

    base = Path(args.path).resolve()
    cs_files = iter_files(base, ".cs")
    json_files = iter_files(base, ".json")

    for _, check_fn in CHECKS:
        if check_fn is check_hardcoded_secrets:
            check_fn(json_files)
        else:
            check_fn(cs_files)

    VIOLATIONS.sort(key=lambda v: (v.file, v.line))

    if args.json:
        print(json.dumps([asdict(v) for v in VIOLATIONS], ensure_ascii=False, indent=2))
    else:
        severity_icon = {"error": "[ERROR]", "warning": "[WARN] ", "info": "[INFO] "}
        for v in VIOLATIONS:
            print(f"{severity_icon.get(v.severity, v.severity):8} {v.rule:28} {v.file}:{v.line} — {v.message}")

        print()
        by_severity = {"error": 0, "warning": 0, "info": 0}
        for v in VIOLATIONS:
            by_severity[v.severity] = by_severity.get(v.severity, 0) + 1

        print(f"Нарушений: {len(VIOLATIONS)} "
              f"(error: {by_severity['error']}, warning: {by_severity['warning']}, info: {by_severity['info']})")

    return 1 if any(v.severity == "error" for v in VIOLATIONS) else 0


if __name__ == "__main__":
    sys.exit(main())
