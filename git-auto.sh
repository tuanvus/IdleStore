#!/bin/bash
# git-auto.sh - Flexible git automation with size checking

# File size limit (in bytes) - 70MB = 73400320 bytes
SIZE_LIMIT=73400320

show_help() {
    echo "Usage: ./git-auto.sh [option] \"commit message\""
    echo "Options:"
    echo "  -a    Only add"
    echo "  -c    Add + commit"
    echo "  -p    Add + commit + push"
    echo ""
    echo "Note: Files larger than 70MB will be automatically excluded"
    exit 0
}

if [ "$1" == "-h" ] || [ "$1" == "--help" ]; then
    show_help
fi

if [ -z "$2" ]; then
    echo "Error: Commit message required!"
    echo "Usage: ./git-auto.sh -p \"your message\""
    exit 1
fi

# Function to check file sizes and create .gitignore entries
check_and_exclude_large_files() {
    echo "🔍 Checking for large files..."
    
    local has_large_files=false
    local excluded_files=()
    
    # Get all files that would be added (modified, new, etc.)
    # Using git ls-files for tracked files and find for untracked
    while IFS= read -r file; do
        # Skip if file doesn't exist (could be deleted)
        if [ ! -f "$file" ]; then
            continue
        fi
        
        # Get file size in bytes
        file_size=$(stat -c%s "$file" 2>/dev/null || stat -f%z "$file" 2>/dev/null)
        
        if [ -z "$file_size" ]; then
            continue
        fi
        
        # Check if file is larger than limit
        if [ "$file_size" -gt "$SIZE_LIMIT" ]; then
            has_large_files=true
            size_mb=$(echo "scale=2; $file_size / 1048576" | bc)
            
            echo "⚠️  Large file detected: $file ($size_mb MB)"
            
            # Add to .gitignore if not already there
            if ! grep -Fxq "$file" .gitignore 2>/dev/null; then
                echo "$file" >> .gitignore
                echo "   ✓ Added to .gitignore"
            fi
            
            excluded_files+=("$file")
            
            # Remove from staging area if already added
            git reset HEAD "$file" 2>/dev/null
        fi
    done < <(git diff --cached --name-only --diff-filter=ACM 2>/dev/null; git ls-files --others --exclude-standard)
    
    if [ "$has_large_files" = true ]; then
        echo ""
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "⚠️  WARNING: Large files detected and excluded!"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "The following files exceed 70MB limit:"
        for file in "${excluded_files[@]}"; do
            echo "  • $file"
        done
        echo ""
        echo "These files have been:"
        echo "  1. Added to .gitignore"
        echo "  2. Removed from staging area"
        echo ""
        echo "Consider using Git LFS for large files:"
        echo "  git lfs track \"*.zip\""
        echo "  git lfs track \"*.rar\""
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo ""
        
        # Ask user if they want to continue
        read -p "Continue with remaining files? (y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "❌ Operation cancelled"
            exit 1
        fi
    else
        echo "✓ No large files detected"
    fi
}

# Function to check specific file extensions (zip, rar, etc.)
check_archive_files() {
    echo "🔍 Checking for archive files..."
    
    local archive_patterns=("*.zip" "*.rar" "*.7z" "*.tar.gz" "*.tar" "*.gz")
    local found_archives=false
    
    for pattern in "${archive_patterns[@]}"; do
        # Check if any files matching pattern exist in staging
        if git diff --cached --name-only --diff-filter=ACM | grep -i "$pattern" > /dev/null 2>&1; then
            found_archives=true
            echo "⚠️  Archive files detected matching: $pattern"
        fi
    done
    
    if [ "$found_archives" = true ]; then
        echo ""
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "⚠️  Archive files will be checked for size limit"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo ""
    fi
}

# Function to show commit summary
show_commit_summary() {
    echo ""
    echo "📊 Commit Summary:"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    
    # Count files to be committed
    local file_count=$(git diff --cached --numstat | wc -l)
    echo "Files to commit: $file_count"
    
    if [ "$file_count" -gt 0 ]; then
        echo ""
        echo "Files:"
        git diff --cached --name-status | while IFS= read -r line; do
            echo "  $line"
        done
        
        echo ""
        echo "Total changes:"
        git diff --cached --shortstat
    fi
    
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
}

case "$1" in
    -a)
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "📦 Adding files..."
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        
        # First, add all files
        git add .
        
        # Then check for large files
        check_and_exclude_large_files
        check_archive_files
        
        # Re-add after exclusions
        git add .
        
        show_commit_summary
        ;;
        
    -c)
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "📦 Adding and committing..."
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        
        # First, add all files
        git add .
        
        # Check for large files
        check_and_exclude_large_files
        check_archive_files
        
        # Re-add after exclusions
        git add .
        
        show_commit_summary
        
        # Check if there are files to commit
        if [ -z "$(git diff --cached --name-only)" ]; then
            echo "❌ No files to commit after excluding large files"
            exit 1
        fi
        
        # Commit
        git commit -m "$2"
        ;;
        
    -p)
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "📦 Adding, committing, and pushing..."
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        
        # First, add all files
        git add .
        
        # Check for large files
        check_and_exclude_large_files
        check_archive_files
        
        # Re-add after exclusions
        git add .
        
        show_commit_summary
        
        # Check if there are files to commit
        if [ -z "$(git diff --cached --name-only)" ]; then
            echo "❌ No files to commit after excluding large files"
            exit 1
        fi
        
        # Commit
        echo "📝 Committing..."
        git commit -m "$2"
        
        if [ $? -eq 0 ]; then
            echo ""
            echo "🚀 Pushing to remote..."
            git push
            
            if [ $? -eq 0 ]; then
                echo ""
                echo "✅ Successfully pushed to remote!"
            else
                echo ""
                echo "❌ Push failed! Please check your connection and try again."
                exit 1
            fi
        else
            echo ""
            echo "❌ Commit failed!"
            exit 1
        fi
        ;;
        
    *)
        echo "Invalid option: $1"
        show_help
        ;;
esac

echo ""
echo "✓ Done!"
