from pathlib import Path

def get_target_entries(file_path):
    """
    Parses a localization file and returns a dictionary of key -> value strings.
    This assumes every key-value pair is kept on a single physical line, 
    which cleanly separates them from structural comments.
    """
    entries = {}
    with open(file_path, 'r', encoding='utf-8') as f:
        for line in f:
            # Check for a new entry line containing '|' (ignoring comments)
            if '|' in line and not line.strip().startswith('//'):
                parts = line.split('|', 1)
                entries[parts[0].strip()] = parts[1]
    return entries

def sync_localization():
    current_dir = Path(__file__).parent
    eng_path = current_dir / "eng.loc"

    if not eng_path.exists():
        print("Error: eng.loc (source of truth) not found in the current directory.")
        return

    print("Parsing source of truth (eng.loc) layout...")
    
    # Read the exact structure of the English file to use as our master template
    with open(eng_path, 'r', encoding='utf-8') as f:
        eng_lines = f.readlines()

    # Find all other .loc files
    loc_files = [f for f in current_dir.glob("*.loc") if f.name != "eng.loc"]

    if not loc_files:
        print("No other .loc files found to sync.")
        return

    for loc_file in loc_files:
        print(f"\nSyncing {loc_file.name}...")
        
        # 1. Grab all existing translations from the target file
        target_entries = get_target_entries(loc_file)
        
        # 2. Extract the target file's header (everything before the first key).
        # This prevents the English header from overwriting the target language declaration.
        target_header = []
        with open(loc_file, 'r', encoding='utf-8') as f:
            for line in f:
                if '|' in line and not line.strip().startswith('//'):
                    break
                target_header.append(line)

        new_lines = []
        missing_count = 0
        
        # Calculate how many header lines are in eng.loc so we can skip them during replication
        eng_header_count = 0
        for line in eng_lines:
            if '|' in line and not line.strip().startswith('//'):
                break
            eng_header_count += 1
            
        # Append the target's original header to start the new file
        new_lines.extend(target_header)
        
        # 3. Rebuild the rest of the file using the English file as the exact template
        for line in eng_lines[eng_header_count:]:
            if '|' in line and not line.strip().startswith('//'):
                parts = line.split('|', 1)
                key = parts[0].strip()
                eng_val = parts[1]
                
                # Check if we have a translation for this key
                if key in target_entries:
                    # Write the key and the target's translated value
                    new_lines.append(f"{key} |{target_entries[key]}")
                else:
                    # Missing entry: Revert to the English value
                    new_lines.append(f"{key} |{eng_val}")
                    missing_count += 1
            else:
                # This is a comment, spacing line, or category header (like EXCLUDED RELICS). 
                # Keep it exactly as it appears in eng.loc.
                new_lines.append(line)

        # 4. Overwrite the target file with the fully synced structure
        with open(loc_file, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
            
        print(f"-> Rebuilt {loc_file.name}: Categorization and layout synced with eng.loc.")
        print(f"-> Missing entries added/reverted to English: {missing_count}")

if __name__ == "__main__":
    sync_localization()