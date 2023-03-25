const std = @import("std");
const lex = @import("lex.zig");
const mem = std.mem;
const process = std.process;
const fs = std.fs;

const Allocator = mem.Allocator;
const File = fs.File;
const Lexer = lex.Lexer;

// A slang file should not be > u32
const max_src_size = std.math.maxInt(u32);
const usage =
    \\Usage: slc [command] [options]
    \\
    \\Commands:
    \\  lex         Lex the specified slang file
    \\
;

var Gpa = std.heap.GeneralPurposeAllocator(.{}){};

pub fn main() !void {
    const gpa = Gpa.allocator();
    defer _ = Gpa.deinit();

    var arena_instance = std.heap.ArenaAllocator.init(gpa);
    defer arena_instance.deinit();
    const arena = arena_instance.allocator();

    const args = try process.argsAlloc(arena);
    defer process.argsFree(arena, args);

    return mainArgs(gpa, arena, args);
}

fn mainArgs(gpa: Allocator, arena: Allocator, args: []const []const u8) !void {
    if (args.len <= 1) {
        std.log.info("{s}", .{usage});
        fatal("Expected command argument", .{});
    }

    const cmd = args[1];
    const cmd_args = args[2..];
    std.log.debug("Executing {s} {s} {s}", .{ args[0], cmd, cmd_args });

    if (mem.eql(u8, cmd, "lex")) {
        return executeLexer(gpa, arena, cmd_args);
    }

    std.log.info("{s}", .{usage});
    fatal("Unknown command: {s}", .{args[1]});
}

fn executeLexer(gpa: Allocator, arena: Allocator, args: []const []const u8) !void {
    if (args.len != 1) {
        fatal("Expected exactly one file argument", .{});
    }

    std.log.debug("Lexing {s}", .{args});

    // Let's make sure we can access the file
    fs.cwd().access(args[0], .{}) catch |err| {
        fatal("Could not find file {s} ({})", .{ args[0], err });
    };

    const filename = args[0];

    const source_file = try fs.cwd().openFile(filename, .{});
    var file_closed = false;
    defer if (!file_closed) source_file.close();

    const stat = try source_file.stat();
    if (stat.kind == .Directory)
        return error.IsDir;

    const source_code = try source_file.readToEndAllocOptions(gpa, max_src_size, stat.size, @alignOf(u8), 0);
    defer gpa.free(source_code);

    var lexer = Lexer.init(source_code);
    while (true) {
        const tok = lexer.next();
        if (tok.tag != .invalid)
            std.log.debug("Token: {}", .{tok});
        if (tok.tag == .eof) break;
    }

    source_file.close();
    file_closed = true;

    _ = arena;
}

fn fatal(comptime format: []const u8, args: anytype) noreturn {
    std.log.err(format, args);
    process.exit(1);
}

// pub fn main() !void {
//     // Prints to stderr (it's a shortcut based on `std.io.getStdErr()`)
//     std.debug.print("All your {s} are belong to us.\n", .{"codebase"});

//     // stdout is for the actual output of your application, for example if you
//     // are implementing gzip, then only the compressed bytes should be sent to
//     // stdout, not any debugging messages.
//     const stdout_file = std.io.getStdOut().writer();
//     var bw = std.io.bufferedWriter(stdout_file);
//     const stdout = bw.writer();

//     try stdout.print("Run `zig build test` to run the tests.\n", .{});

//     try bw.flush(); // don't forget to flush!
// }

// test "simple test" {
//     var list = std.ArrayList(i32).init(std.testing.allocator);
//     defer list.deinit(); // try commenting this out and see if zig detects the memory leak!
//     try list.append(42);
//     try std.testing.expectEqual(@as(i32, 42), list.pop());
// }
